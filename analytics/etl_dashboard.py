# Analytics ETL for ComputerStore Dashboard
# Requires: pip install psycopg[binary] pandas numpy

import os
import json
import psycopg
import pandas as pd
import numpy as np
from datetime import datetime

DB_DSN = os.getenv('DATABASE_URL', 'host=localhost port=5432 dbname=ComputerStoreDB user=postgres password=Abcdefghij123 connect_timeout=10 sslmode=prefer')


def get_conn():
    return psycopg.connect(DB_DSN)


def fetch_df(sql: str):
    with get_conn() as conn:
        return pd.read_sql(sql, conn)


def kpis():
    df_orders = fetch_df(
        'select "Id", "Total", "Estado", "MetodoPago", "FechaCreacion"::date as fecha from "Pedidos"'
    )
    df_tx = fetch_df(
        'select "TransactionId" as transaction_id, "Estado" as estado, "Monto" as monto, "FechaTransaccion"::date as fecha from "Transacciones"'
    )

    df_orders['Total'] = pd.to_numeric(df_orders['Total']).fillna(0)
    df_tx['monto'] = pd.to_numeric(df_tx['monto']).fillna(0)

    today = pd.Timestamp(datetime.utcnow().date())

    ventas_mes = df_orders[(df_orders['fecha'] >= today.replace(day=1)) & (df_orders['Estado'].isin(['Pagado','Completado','Entregado']))]['Total'].sum()
    ventas_totales = df_orders[df_orders['Estado'].isin(['Pagado','Completado','Entregado'])]['Total'].sum()
    pedidos_pendientes = int((df_orders['Estado'] == 'Pendiente').sum())
    pedidos_completados = int(df_orders['Estado'].isin(['Pagado','Completado','Entregado']).sum())

    tx_aprobadas = int((df_tx['estado'] == 'APPROVED').sum())
    tx_pendientes = int((df_tx['estado'] == 'PENDING').sum())
    tx_rechazadas = int(df_tx['estado'].isin(['REJECTED','DECLINED']).sum())
    monto_tx_aprobadas = df_tx.loc[df_tx['estado'] == 'APPROVED', 'monto'].sum()

    return {
        'ventas_mes': float(ventas_mes),
        'ventas_totales': float(ventas_totales),
        'pedidos_pendientes': pedidos_pendientes,
        'pedidos_completados': pedidos_completados,
        'tx_aprobadas': tx_aprobadas,
        'tx_pendientes': tx_pendientes,
        'tx_rechazadas': tx_rechazadas,
        'monto_tx_aprobadas': float(monto_tx_aprobadas),
    }


def series_ventas_mensuales():
    df = fetch_df('select "FechaCreacion"::date as fecha, "Total", "Estado" from "Pedidos"')
    df['Total'] = pd.to_numeric(df['Total']).fillna(0)
    df_ok = df[df['Estado'].isin(['Pagado','Completado','Entregado'])]
    if df_ok.empty:
        return {'labels': [], 'values': []}
    s = df_ok.groupby(df_ok['fecha'].astype('datetime64[M]'))['Total'].sum()
    s = s.sort_index()
    return {'labels': [d.strftime('%Y-%m') for d in s.index], 'values': [float(v) for v in s.values]}


def metodos_pago_distribution():
    df = fetch_df('select "MetodoPago" from "Transacciones"')
    if df.empty:
        return {'labels': [], 'values': []}
    counts = df['MetodoPago'].fillna('DESCONOCIDO').value_counts()
    return {'labels': counts.index.tolist(), 'values': counts.values.tolist()}


def pedidos_por_estado():
    df = fetch_df('select "Estado" from "Pedidos"')
    if df.empty:
        return {'labels': [], 'values': []}
    counts = df['Estado'].value_counts()
    return {'labels': counts.index.tolist(), 'values': counts.values.tolist()}


def top_productos(limit: int = 10):
    # Sumar cantidades por producto a partir de detalles de pedido
    sql = (
        'select d."ProductoId", sum(d."Cantidad") as cantidad '
        'from "PedidoDetalles" d '
        'group by d."ProductoId" '
        'order by cantidad desc '
        f'limit {limit}'
    )
    df_qty = fetch_df(sql)
    if df_qty.empty:
        return {'labels': [], 'values': []}
    ids = ','.join(str(int(x)) for x in df_qty['productoId'].tolist()) if 'productoId' in df_qty.columns else ','.join(str(int(x)) for x in df_qty['ProductoId'].tolist())
    df_names = fetch_df(f'select "Id", "Name" from "Productos" where "Id" in ({ids})')
    name_map = {int(r['Id']): r['Name'] for _, r in df_names.iterrows()}
    labels = []
    values = []
    for _, r in df_qty.iterrows():
        pid = int(r['ProductoId'] if 'ProductoId' in r else r['productoid'])
        labels.append(name_map.get(pid, f'Producto {pid}'))
        values.append(int(r['cantidad']))
    return {'labels': labels, 'values': values}


if __name__ == '__main__':
    print(json.dumps({
        'kpis': kpis(),
        'ventas_mensuales': series_ventas_mensuales(),
        'metodos_pago': metodos_pago_distribution(),
        'pedidos_estados': pedidos_por_estado(),
        'top_productos': top_productos(8)
    }, indent=2, ensure_ascii=False))
