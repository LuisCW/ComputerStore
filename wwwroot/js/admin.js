/**
 * CompuHiperMegaRed Admin Panel JavaScript
 * Funcionalidades para el panel de administrador
 */

(function($) {
    "use strict";

    function toggleSidebar(){
        $('body').toggleClass('sidebar-toggled');
        console.log('sidebar toggled ->', $('body').hasClass('sidebar-toggled'));
    }

    $(function(){
        if(!document.getElementById('sidebarToggleMain')){
            // Fallback: attach to #sidebarToggleTop only
            console.warn('#sidebarToggleMain not found; using top button');
        }
        $(document).on('click', '#sidebarToggle, #sidebarToggleTop, #sidebarToggleMain', function(){ toggleSidebar(); });
    });

    // Close any open menu accordions when window is resized below 768px
    $(window).resize(function() {
        if ($(window).width() < 768) {
            $('.sidebar .collapse').collapse('hide');
        }
    });

    // Prevent the content wrapper from scrolling when the fixed side navigation hovered over
    $('body.fixed-nav .sidebar').on('mousewheel DOMMouseScroll wheel', function(e) {
        if ($(window).width() > 768) {
            var e0 = e.originalEvent,
                delta = e0.wheelDelta || -e0.detail;
            this.scrollTop += (delta < 0 ? 1 : -1) * 30;
            e.preventDefault();
        }
    });

    // Scroll to top button appear
    $(document).on('scroll', function() {
        var scrollDistance = $(this).scrollTop();
        if (scrollDistance > 100) { $('.scroll-to-top').fadeIn(); } else { $('.scroll-to-top').fadeOut(); }
    });

    $(document).on('click', 'a.scroll-to-top', function(e) {
        var $anchor = $(this);
        $('html, body').stop().animate({ scrollTop: ($($anchor.attr('href')).offset().top) }, 1000);
        e.preventDefault();
    });

    try {
        if (window.bootstrap && bootstrap.Tooltip) {
            document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function(el){ new bootstrap.Tooltip(el); });
            document.querySelectorAll('[data-bs-toggle="popover"]').forEach(function(el){ new bootstrap.Popover(el); });
        } else if ($ && $.fn && $.fn.tooltip) {
            $('[data-toggle="tooltip"]').tooltip();
            $('[data-toggle="popover"]').popover();
        }
    } catch(ex) { console.warn('Tooltip/Popover init error', ex); }

    setTimeout(function() { $('.alert').fadeOut('slow'); }, 5000);

    if ($.fn && $.fn.DataTable) {
        $('.admin-table').DataTable({ language: { url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json' }, responsive: true, pageLength: 25, order: [[0, 'desc']] });
    }

    $('.admin-form-ajax').on('submit', function(e) {
        e.preventDefault();
        var $form = $(this);
        var $submitBtn = $form.find('button[type="submit"]');
        var originalBtnText = $submitBtn.html();
        $submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Procesando...');
        $.ajax({ url: $form.attr('action'), type: $form.attr('method') || 'POST', data: $form.serialize(), success: function(response) {
            if (response && response.success) { showAlert('success', response.message || 'Operación completada exitosamente'); if (response.redirect) { window.location.href = response.redirect; } }
            else { showAlert('danger', (response && response.message) || 'Error al procesar la solicitud'); }
        }, error: function() { showAlert('danger', 'Error de conexión. Por favor intente nuevamente.'); }, complete: function() { $submitBtn.prop('disabled', false).html(originalBtnText); } });
    });

    function showAlert(type, message) {
        var icon = (type === 'success') ? 'check-circle' : 'exclamation-circle';
        var alertHtml = '<div class="alert alert-'+type+' alert-dismissible fade show" role="alert">'+ '<i class="fas fa-'+icon+' me-2"></i>'+ message + '<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>';
        $('#alerts-container').prepend(alertHtml);
        setTimeout(function() { $('.alert').first().fadeOut('slow'); }, 5000);
    }

    $('.delete-confirm').on('click', function(e) { e.preventDefault(); var $this = $(this); var message = $this.data('message') || '¿Está seguro de que desea eliminar este elemento?'; if (confirm(message)) { if ($this.closest('form').length) { $this.closest('form').submit(); } else { window.location.href = $this.attr('href'); } } });

    window.AdminRealTime = { init: function() { this.updateStats(); setInterval(this.updateStats, 30000); }, updateStats: function() { $.get('/Admin/Api/Stats', function(data) { if (data) { $('.stat-ventas-hoy').text(data.ventasHoy); $('.stat-pedidos-hoy').text(data.pedidosHoy); $('.stat-envios-activos').text(data.enviosActivos); $('.stat-productos-bajo-stock').text(data.productosBajoStock); } }); } };

    $(document).ready(function() { window.Admin = window.Admin || {}; window.Admin.toggleSidebar = toggleSidebar; });

})(jQuery);