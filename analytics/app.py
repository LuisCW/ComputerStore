# FastAPI service for analytics endpoints
# Requires: pip install fastapi uvicorn psycopg[binary] pandas numpy
# Environment:
#   DATABASE_URL=host=127.0.0.1 port=5432 dbname=ComputerStoreDB user=postgres password=Abcdefghij123 connect_timeout=10 sslmode=prefer

from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse, PlainTextResponse
from fastapi.middleware.cors import CORSMiddleware
import os
import psycopg
import pandas as pd
from datetime import datetime, timedelta
import json

app = FastAPI(title="ComputerStore Analytics API", version="1.2.0")

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


def _cn_from_appsettings() -> str | None:
    paths = [
        os.path.join(os.getcwd(), 'appsettings.Development.json'),
        os.path.join(os.getcwd(), 'appsettings.json'),
    ]
    for p in paths:
        if os.path.exists(p):
            try:
                with open(p, 'r', encoding='utf-8') as f:
                    cfg = json.load(f)
                conn = cfg.get('ConnectionStrings', {}).get('DefaultConnection')
                if not conn:
                    continue
                kv = {}
                for part in conn.split(';'):
                    if not part.strip() or '=' not in part:
                        continue
                    k, v = part.split('=', 1)
                    kv[k.strip().lower()] = v.strip()
                host = kv.get('host') or kv.get('server') or '127.0.0.1'
                db = kv.get('database') or kv.get('dbname') or 'postgres'
                user = kv.get('username') or kv.get('user') or 'postgres'
                pwd = kv.get('password') or kv.get('pwd') or ''
                port = kv.get('port') or '5432'
                return f"host={host} port={port} dbname={db} user={user} password={pwd} connect_timeout=10 sslmode=prefer"
            except Exception:
                continue
    return None

DB_DSN = os.getenv('DATABASE_URL') or _cn_from_appsettings() or 'host=127.0.0.1 port=5432 dbname=ComputerStoreDB user=postgres password=Abcdefghij123 connect_timeout=10 sslmode=prefer'


def fetch_df(sql: str) -> pd.DataFrame:
    with psycopg.connect(DB_DSN) as conn:
        with conn.cursor() as cur:
            cur.execute(sql)
            rows = cur.fetchall()
            cols = [d[0] for d in cur.description] if cur.description else []
            return pd.DataFrame(rows, columns=cols)


OK_STATES = {"PAGADO", "COMPLETADO", "ENTREGADO"}


def table_exists(name: str) -> bool:
    try:
        df = fetch_df("select 1 from information_schema.tables where table_name=%s limit 1" % ("'"+name.lower()+"'"))
        return not df.empty
    except Exception:
        return False


def build_payload_resilient():
    errors: list[str] = []

    def safe_fetch(sql: str, tag: str) -> pd.DataFrame:
        try:
            return fetch_df(sql)
        except Exception as e:
            errors.append(f"{tag}: {e}")
            return pd.DataFrame()

    # Pedidos
    df_orders = safe_fetch('select "Id", "UserId", "Total", upper("Estado") as Estado, "MetodoPago", "FechaCreacion"::date as fecha from "Pedidos"', 'Q_ORDERS')
    # Transacciones (manejar ausencia de columna "Metodo")
    df_tx = safe_fetch('select "TransactionId" as transaction_id, upper("Estado") as estado, "Monto" as monto, "MetodoPago" as MetodoPago, "FechaTransaccion"::date as fecha from "Transacciones"', 'Q_TX')
    if df_tx.empty:
        # reintento con nombre alterno de columna de método
        df_tx = safe_fetch('select "TransactionId" as transaction_id, upper("Estado") as estado, "Monto" as monto, "Metodo" as MetodoPago, "FechaTransaccion"::date as fecha from "Transacciones"', 'Q_TX_ALT')

    # Normalizar numéricos y fechas
    if 'Total' in df_orders.columns:
        df_orders['Total'] = pd.to_numeric(df_orders['Total'], errors='coerce').fillna(0)
    if 'monto' in df_tx.columns:
        df_tx['monto'] = pd.to_numeric(df_tx['monto'], errors='coerce').fillna(0)
    if 'fecha' in df_orders.columns:
        df_orders['fecha'] = pd.to_datetime(df_orders['fecha'], errors='coerce')
    if 'fecha' in df_tx.columns:
        df_tx['fecha'] = pd.to_datetime(df_tx['fecha'], errors='coerce')

    today_date = datetime.utcnow().date()
    start_month = pd.Timestamp(today_date.replace(day=1))

    try:
        mask_ok = df_orders['Estado'].isin(list(OK_STATES)) if 'Estado' in df_orders.columns else pd.Series(dtype=bool)
        ventas_mes = df_orders[(df_orders.get('fecha', pd.Series(dtype='datetime64[ns]')) >= start_month) & mask_ok]['Total'].sum() if not df_orders.empty else 0
        ventas_totales = df_orders[mask_ok]['Total'].sum() if 'Estado' in df_orders.columns else 0
        pedidos_pendientes = int((df_orders['Estado'] == 'PENDIENTE').sum()) if 'Estado' in df_orders.columns else 0
        pedidos_completados = int(mask_ok.sum()) if 'Estado' in df_orders.columns else 0
    except Exception as e:
        errors.append(f"KPIS: {e}")
        ventas_mes = ventas_totales = 0
        pedidos_pendientes = pedidos_completados = 0

    try:
        tx_aprobadas = int((df_tx['estado'] == 'APPROVED').sum()) if 'estado' in df_tx.columns else 0
        tx_pendientes = int((df_tx['estado'] == 'PENDING').sum()) if 'estado' in df_tx.columns else 0
        tx_rechazadas = int(df_tx['estado'].isin(['REJECTED','DECLINED']).sum()) if 'estado' in df_tx.columns else 0
        monto_tx_aprobadas = df_tx.loc[df_tx['estado'] == 'APPROVED', 'monto'].sum() if {'estado','monto'}.issubset(df_tx.columns) else 0
    except Exception as e:
        errors.append(f"TX_KPIS: {e}")
        tx_aprobadas = tx_pendientes = tx_rechazadas = 0
        monto_tx_aprobadas = 0

    # Ventas mensuales (barras) - últimos 12 meses reindexado
    df_vm = safe_fetch('select "FechaCreacion"::date as fecha, "Total", upper("Estado") as Estado from "Pedidos"', 'Q_VM')
    if not df_vm.empty:
        df_vm['Total'] = pd.to_numeric(df_vm['Total'], errors='coerce').fillna(0)
        df_vm['fecha'] = pd.to_datetime(df_vm['fecha'], errors='coerce')
    try:
        months = pd.period_range(pd.Timestamp(today_date).to_period('M') - 11, pd.Timestamp(today_date).to_period('M'), freq='M')
        if df_vm.empty or not {'fecha','Total','Estado'}.issubset(df_vm.columns):
            ventas_mensuales = {'labels': [str(m) for m in months], 'values': [0.0]*len(months)}
            df_ok = pd.DataFrame(columns=['fecha','Total'])
        else:
            df_ok = df_vm[df_vm['Estado'].isin(list(OK_STATES))]
            per = pd.to_datetime(df_ok['fecha']).dt.to_period('M') if not df_ok.empty else pd.Series([], dtype='period[M]')
            s = df_ok.groupby(per)['Total'].sum() if not df_ok.empty else pd.Series(dtype=float)
            s = s.reindex(months, fill_value=0)
            ventas_mensuales = {'labels': [str(p) for p in s.index], 'values': [float(v) for v in s.values]}
    except Exception as e:
        errors.append(f"VENTAS_MENSUALES: {e}")
        months = pd.period_range(pd.Timestamp(today_date).to_period('M') - 11, pd.Timestamp(today_date).to_period('M'), freq='M')
        ventas_mensuales = {'labels': [str(m) for m in months], 'values': [0.0]*len(months)}
        df_ok = pd.DataFrame(columns=['fecha','Total'])

    # Ingresos diarios últimos 30 días (línea)
    try:
        fecha_min = pd.Timestamp(today_date - timedelta(days=29))
        df_30 = df_orders[(df_orders.get('fecha', pd.Series(dtype='datetime64[ns]')) >= fecha_min) & (df_orders['Estado'].isin(list(OK_STATES)) if 'Estado' in df_orders.columns else False)] if not df_orders.empty else pd.DataFrame(columns=['fecha','Total'])
        if df_30.empty:
            ingresos_diarios_30 = {'labels': [], 'values': []}
        else:
            s = df_30.groupby(pd.to_datetime(df_30['fecha']).dt.date)['Total'].sum()
            rng = pd.date_range(fecha_min, pd.Timestamp(today_date), freq='D').date
            s = s.reindex(rng, fill_value=0)
            ingresos_diarios_30 = {'labels': [d.strftime('%Y-%m-%d') for d in s.index], 'values': [float(v) for v in s.values]}
    except Exception as e:
        errors.append(f"INGRESOS_30: {e}")
        ingresos_diarios_30 = {'labels': [], 'values': []}

    # Pedidos por estado (doughnut)
    df_est = safe_fetch('select upper("Estado") as Estado from "Pedidos"', 'Q_EST')
    try:
        if df_est.empty or 'Estado' not in df_est.columns:
            pedidos_estados = {'labels': [], 'values': []}
        else:
            counts = df_est['Estado'].astype(str).value_counts()
            pedidos_estados = {'labels': counts.index.tolist(), 'values': counts.values.tolist()}
    except Exception as e:
        errors.append(f"PEDIDOS_ESTADOS: {e}")
        pedidos_estados = {'labels': [], 'values': []}

    # Top productos por cantidad (barras)
    df_qty = safe_fetch('select d."ProductoId", sum(d."Cantidad") as cantidad from "PedidoDetalles" d group by d."ProductoId" order by cantidad desc limit 8', 'Q_TOP')
    try:
        if df_qty.empty or not {'ProductoId','cantidad'}.issubset(df_qty.columns):
            top_productos = {'labels': [], 'values': []}
        else:
            ids = ','.join(str(int(x)) for x in df_qty['ProductoId'].tolist())
            df_names = safe_fetch(f'select "Id", "Name" from "Productos" where "Id" in ({ids})', 'Q_TOP_NAMES') if ids else pd.DataFrame()
            name_map = {int(r['Id']): r['Name'] for _, r in df_names.iterrows()} if not df_names.empty else {}
            labels = []
            values = []
            for _, r in df_qty.iterrows():
                pid = int(r['ProductoId'])
                labels.append(name_map.get(pid, f'Producto {pid}'))
                values.append(int(r['cantidad']))
            top_productos = {'labels': labels, 'values': values}
    except Exception as e:
        errors.append(f"TOP_PRODUCTOS: {e}")
        top_productos = {'labels': [], 'values': []}

    # Métodos de pago por monto (barras)
    try:
        if df_tx.empty or not {'MetodoPago','monto'}.issubset(df_tx.columns):
            ventas_por_metodo_monto = {'labels': [], 'values': []}
        else:
            s = df_tx.groupby(df_tx['MetodoPago'].fillna('DESCONOCIDO'))['monto'].sum().sort_values(ascending=False)
            ventas_por_metodo_monto = {'labels': s.index.astype(str).tolist(), 'values': [float(v) for v in s.values]}
    except Exception as e:
        errors.append(f"VENTAS_METODO_MONTO: {e}")
        ventas_por_metodo_monto = {'labels': [], 'values': []}

    # Métodos de pago por conteo (doughnut)
    try:
        if df_tx.empty or 'MetodoPago' not in df_tx.columns:
            metodos_pago = {'labels': [], 'values': []}
        else:
            c = df_tx['MetodoPago'].fillna('DESCONOCIDO').value_counts()
            metodos_pago = {'labels': c.index.astype(str).tolist(), 'values': c.values.astype(int).tolist()}
    except Exception as e:
        errors.append(f"METODOS_PAGO: {e}")
        metodos_pago = {'labels': [], 'values': []}

    # Ventas por categoría por mes (stacked)
    df_catm = safe_fetch("select date_trunc('month', p.\"FechaCreacion\")::date as mes, pr.\"Category\" as categoria, sum(d.\"Subtotal\") as monto from \"PedidoDetalles\" d join \"Pedidos\" p on p.\"Id\"=d.\"PedidoId\" join \"Productos\" pr on pr.\"Id\"=d.\"ProductoId\" where upper(p.\"Estado\") in ('PAGADO','COMPLETADO','ENTREGADO') group by mes, categoria order by mes, categoria", 'Q_CAT_M')
    ventas_por_categoria_mes = {'labels': [], 'series': []}
    try:
        if not df_catm.empty:
            df_catm['monto'] = pd.to_numeric(df_catm['monto'], errors='coerce').fillna(0)
            df_catm['mes'] = pd.to_datetime(df_catm['mes'], errors='coerce')
            df_pivot = df_catm.pivot_table(index='mes', columns='categoria', values='monto', aggfunc='sum').fillna(0).sort_index()
            ventas_por_categoria_mes['labels'] = [pd.Timestamp(ix).strftime('%Y-%m') for ix in df_pivot.index]
            for cat in df_pivot.columns:
                ventas_por_categoria_mes['series'].append({'label': str(cat), 'values': [float(v) for v in df_pivot[cat].values]})
    except Exception as e:
        errors.append(f"VENTAS_CAT_MES: {e}")

    # Ticket promedio mensual (AOV)
    ticket_promedio_mensual = {'labels': [], 'values': []}
    try:
        if not df_ok.empty:
            per = pd.to_datetime(df_ok['fecha']).dt.to_period('M')
            s = df_ok.groupby(per)['Total'].mean().sort_index()
            ticket_promedio_mensual = {'labels': [str(p) for p in s.index], 'values': [float(v) for v in s.values]}
    except Exception as e:
        errors.append(f"TICKET_PROMEDIO: {e}")

    # Top clientes por monto
    df_top_cli = safe_fetch('select coalesce(u."UserName", u."Email", p."UserId") as cliente, sum(p."Total") as monto from "Pedidos" p left join "AspNetUsers" u on u."Id" = p."UserId" where upper(p."Estado") in (\'PAGADO\',\'COMPLETADO\',\'ENTREGADO\') group by cliente order by monto desc limit 10', 'Q_TOP_CLIENTS')
    top_clientes_monto = {'labels': [], 'values': []}
    try:
        if not df_top_cli.empty:
            df_top_cli['monto'] = pd.to_numeric(df_top_cli['monto'], errors='coerce').fillna(0)
            top_clientes_monto = {'labels': df_top_cli['cliente'].astype(str).tolist(), 'values': [float(v) for v in df_top_cli['monto'].tolist()]}
    except Exception as e:
        errors.append(f"TOP_CLIENTES: {e}")

    # Clientes nuevos por mes
    df_users = safe_fetch('select "FechaRegistro"::date as fecha from "AspNetUsers"', 'Q_USERS')
    clientes_nuevos_mes = {'labels': [], 'values': []}
    try:
        if not df_users.empty:
            per = pd.to_datetime(df_users['fecha']).dt.to_period('M')
            s = per.value_counts().sort_index()
            clientes_nuevos_mes = {'labels': [str(p) for p in s.index], 'values': [int(v) for v in s.values]}
    except Exception as e:
        errors.append(f"CLIENTES_NUEVOS: {e}")

    # Recompra: clientes 1 compra vs 2+
    recompr = {'labels': ['1 compra','2+ compras'], 'values': [0,0]}
    try:
        if not df_orders.empty and 'UserId' in df_orders.columns and 'Estado' in df_orders.columns:
            cnt = df_orders[df_orders['Estado'].isin(list(OK_STATES))].groupby('UserId')['Id'].count()
            one = int((cnt == 1).sum())
            two_plus = int((cnt >= 2).sum())
            recompr = {'labels': ['1 compra','2+ compras'], 'values': [one, two_plus]}
    except Exception as e:
        errors.append(f"RECOMPRA: {e}")

    # Stock bajo por categoría
    df_stock = safe_fetch('select "Category" as categoria, sum(case when "Stock" <= coalesce("StockMinimo",0) then 1 else 0 end) as bajo from "Productos" group by "Category"', 'Q_STOCK')
    stock_bajo_por_categoria = {'labels': [], 'values': []}
    try:
        if not df_stock.empty:
            stock_bajo_por_categoria = {'labels': df_stock['categoria'].astype(str).tolist(), 'values': [int(x) for x in df_stock['bajo'].tolist()]}
    except Exception as e:
        errors.append(f"STOCK_BAJO: {e}")

    # Transacciones: diarias 30 y por estado
    tx_diarias_30 = {'labels': [], 'values': []}
    tx_estado_total = {'labels': [], 'values': []}
    try:
        if not df_tx.empty and 'fecha' in df_tx.columns:
            fecha_min = pd.Timestamp(today_date - timedelta(days=29))
            s = df_tx[pd.to_datetime(df_tx['fecha']) >= fecha_min].groupby(pd.to_datetime(df_tx['fecha']).dt.date)['transaction_id'].count()
            rng = pd.date_range(fecha_min, pd.Timestamp(today_date), freq='D').date
            s = s.reindex(rng, fill_value=0)
            tx_diarias_30 = {'labels': [d.strftime('%Y-%m-%d') for d in s.index], 'values': [int(v) for v in s.values]}
            est = df_tx['estado'].value_counts()
            tx_estado_total = {'labels': est.index.astype(str).tolist(), 'values': est.values.astype(int).tolist()}
    except Exception as e:
        errors.append(f"TX_SERIES: {e}")

    # Ubicaciones (puede no existir columnas en pedidos)
    compras_por_ciudad = {'labels': [], 'values': []}
    compras_por_departamento = {'labels': [], 'values': []}
    clientes_por_ciudad = {'labels': [], 'values': []}
    pedidos_30d_por_ciudad = {'labels': [], 'values': []}
    try:
        if not df_orders.empty and {'CiudadEnvio','DepartamentoEnvio','Estado','fecha','Id'}.issubset(set(df_orders.columns.tolist()+['CiudadEnvio','DepartamentoEnvio'])):
            df_loc = safe_fetch('select "Id","Total",upper("Estado") as Estado,"CiudadEnvio","DepartamentoEnvio","FechaCreacion"::date as fecha from "Pedidos"', 'Q_PED_LOC')
            if not df_loc.empty:
                df_loc['Total'] = pd.to_numeric(df_loc['Total'], errors='coerce').fillna(0)
                df_loc['fecha'] = pd.to_datetime(df_loc['fecha'], errors='coerce')
                ok_mask = df_loc['Estado'].isin(list(OK_STATES))
                cp_ci = df_loc[ok_mask & df_loc['CiudadEnvio'].notna()].groupby('CiudadEnvio')['Total'].sum().sort_values(ascending=False)
                compras_por_ciudad = {'labels': cp_ci.index.astype(str).tolist(), 'values': [float(v) for v in cp_ci.values]}
                cp_de = df_loc[ok_mask & df_loc['DepartamentoEnvio'].notna()].groupby('DepartamentoEnvio')['Total'].sum().sort_values(ascending=False)
                compras_por_departamento = {'labels': cp_de.index.astype(str).tolist(), 'values': [float(v) for v in cp_de.values]}
                fecha_min30 = pd.Timestamp(today_date - timedelta(days=29))
                ped30 = df_loc[ok_mask & (df_loc['fecha'] >= fecha_min30)]
                ped30c = ped30[ped30['CiudadEnvio'].notna()].groupby('CiudadEnvio')['Id'].count().sort_values(ascending=False)
                pedidos_30d_por_ciudad = {'labels': ped30c.index.astype(str).tolist(), 'values': ped30c.values.astype(int).tolist()}
        df_users_city = safe_fetch('select "Ciudad" from "AspNetUsers" where "Ciudad" is not null and length("Ciudad")>0', 'Q_USERS_CITY')
        if not df_users_city.empty:
            cpc = df_users_city['Ciudad'].astype(str).value_counts()
            clientes_por_ciudad = {'labels': cpc.index.tolist(), 'values': cpc.values.astype(int).tolist()}
    except Exception as e:
        errors.append(f"UBICACIONES: {e}")

    # Tráfico real si existe tabla TrafficEvents
    traffic_diario_30 = {'labels': [], 'visitas': [], 'usuarios': []}
    traffic_top_paths = {'labels': [], 'values': []}
    traffic_device = {'labels': [], 'values': []}
    try:
        if table_exists('TrafficEvents'):
            df_tr = safe_fetch("select \"Fecha\",\"Path\",\"UserId\",\"SessionId\",\"Device\" from \"TrafficEvents\" where \"Fecha\" >= now() - interval '30 day'", 'Q_TRAFFIC')
            if not df_tr.empty:
                df_tr['Fecha'] = pd.to_datetime(df_tr['Fecha'], errors='coerce')
                df_tr['day'] = df_tr['Fecha'].dt.date
                s_vis = df_tr.groupby('day')['SessionId'].nunique().sort_index()
                s_usr = df_tr.groupby('day')['UserId'].nunique().sort_index()
                full_rng = pd.date_range(datetime.utcnow().date() - timedelta(days=29), datetime.utcnow().date(), freq='D').date
                s_vis = s_vis.reindex(full_rng, fill_value=0)
                s_usr = s_usr.reindex(full_rng, fill_value=0)
                traffic_diario_30 = {'labels': [d.strftime('%Y-%m-%d') for d in full_rng], 'visitas': s_vis.values.astype(int).tolist(), 'usuarios': s_usr.values.astype(int).tolist()}
                top_paths = df_tr['Path'].value_counts().head(15)
                traffic_top_paths = {'labels': top_paths.index.astype(str).tolist(), 'values': top_paths.values.astype(int).tolist()}
                dev = df_tr['Device'].fillna('Desconocido').value_counts()
                traffic_device = {'labels': dev.index.astype(str).tolist(), 'values': dev.values.astype(int).tolist()}
    except Exception as e:
        errors.append(f"TRAFICO: {e}")

    return {
        'source': 'python',
        'kpis': {
            'ventas_mes': float(ventas_mes),
            'ventas_totales': float(ventas_totales),
            'pedidos_pendientes': pedidos_pendientes,
            'pedidos_completados': pedidos_completados,
            'tx_aprobadas': tx_aprobadas,
            'tx_pendientes': tx_pendientes,
            'tx_rechazadas': tx_rechazadas,
            'monto_tx_aprobadas': float(monto_tx_aprobadas),
        },
        'ventas_mensuales': ventas_mensuales,
        'ingresos_diarios_30': ingresos_diarios_30,
        'pedidos_estados': pedidos_estados,
        'metodos_pago': metodos_pago,
        'ventas_por_metodo_monto': ventas_por_metodo_monto,
        'top_productos': top_productos,
        'ventas_por_categoria_mes': ventas_por_categoria_mes,
        'ticket_promedio_mensual': ticket_promedio_mensual,
        'top_clientes_monto': top_clientes_monto,
        'clientes_nuevos_mes': clientes_nuevos_mes,
        'recompra': recompr,
        'stock_bajo_por_categoria': stock_bajo_por_categoria,
        'tx_diarias_30': tx_diarias_30,
        'tx_estado_total': tx_estado_total,
        'compras_por_ciudad': compras_por_ciudad,
        'compras_por_departamento': compras_por_departamento,
        'clientes_por_ciudad': clientes_por_ciudad,
        'pedidos_30d_por_ciudad': pedidos_30d_por_ciudad,
        'traffic_diario_30': traffic_diario_30,
        'traffic_top_paths': traffic_top_paths,
        'traffic_device': traffic_device,
        'errors': errors,
        'using_dsn': DB_DSN,
    }


@app.get('/')
async def root():
    return JSONResponse({
        'service': 'ComputerStore Analytics API',
        'version': '1.2.0',
        'health': '/healthz',
        'analytics': '/api/analytics',
        'dsn': DB_DSN
    })


@app.get('/favicon.ico')
async def favicon():
    return PlainTextResponse('', status_code=204)


@app.get('/healthz')
async def healthz():
    try:
        with psycopg.connect(DB_DSN) as conn:
            with conn.cursor() as cur:
                cur.execute('select 1')
                cur.fetchone()
        return JSONResponse({'status': 'ok', 'db': 'connected'})
    except Exception as ex:
        return JSONResponse({'status': 'error', 'db_error': str(ex)}, status_code=500)


@app.get('/api/analytics')
async def analytics(req: Request):
    payload = build_payload_resilient()
    return JSONResponse(payload)
