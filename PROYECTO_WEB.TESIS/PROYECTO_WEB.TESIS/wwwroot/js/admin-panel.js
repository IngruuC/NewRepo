// wwwroot/js/admin-panel.js — versión final completa

// ═══════════════════════════════════════════════
// NAVEGACIÓN
// ═══════════════════════════════════════════════
function showPanel(name, btn) {
    document.querySelectorAll('.panel').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.nav-btn').forEach(b => b.classList.remove('active'));
    var panel = document.getElementById('panel-' + name);
    if (panel) panel.classList.add('active');
    if (btn) btn.classList.add('active');
    // Si es seguridad, ocultar sub-secciones
    if (name === 'seguridad') ocultarSecciones();
}

// ═══════════════════════════════════════════════
// FILTROS GENÉRICOS
// ═══════════════════════════════════════════════
function filtrarTabla(tableId, value, colIndexes) {
    var rows = document.querySelectorAll('#' + tableId + ' tbody tr');
    var q = (value || '').toLowerCase().trim();
    rows.forEach(function (row) {
        if (!q) { row.style.display = ''; return; }
        var texto = '';
        colIndexes.forEach(function (i) {
            var cell = row.cells[i];
            if (cell) texto += cell.textContent.toLowerCase() + ' ';
        });
        row.style.display = texto.includes(q) ? '' : 'none';
    });
}

function filtrarClientes(v) { filtrarTabla('tabla-clientes', v, [0, 1, 2]); }
function filtrarProductos(v) { filtrarTabla('tabla-productos', v, [1, 2]); }
function filtrarProveedores(v) { filtrarTabla('tabla-proveedores', v, [1, 2]); }
function filtrarVentas(v) { filtrarTabla('tabla-ventas', v, [2, 3]); recalcTotalVentas(); }
function filtrarCompras(v) { filtrarTabla('tabla-compras', v, [2, 3, 4]); recalcTotalCompras(); }

// ═══════════════════════════════════════════════
// MODAL GENÉRICO
// ═══════════════════════════════════════════════
function abrirModal(titulo, contenido) {
    document.getElementById('modal-title').textContent = titulo;
    document.getElementById('modal-body').textContent = contenido;
    document.getElementById('modal-overlay').style.display = 'flex';
}
function cerrarModal() {
    document.getElementById('modal-overlay').style.display = 'none';
}
document.addEventListener('click', function (e) {
    var overlay = document.getElementById('modal-overlay');
    if (overlay && e.target === overlay) cerrarModal();
});

// ═══════════════════════════════════════════════
// CLIENTES — modo guardar / actualizar unificado
// ═══════════════════════════════════════════════
var clienteSeleccionadoId = null;
var clienteModoModificacion = false;

function seleccionarCliente(row, id, doc, nombre, apellido, dir) {
    document.querySelectorAll('#tabla-clientes tbody tr').forEach(r => r.classList.remove('selected'));
    row.classList.add('selected');
    clienteSeleccionadoId = id;
    document.getElementById('cli-doc').value = doc;
    document.getElementById('cli-nombre').value = nombre;
    document.getElementById('cli-apellido').value = apellido;
    document.getElementById('cli-dir').value = dir;
    document.getElementById('h-cli-id').value = id;
    if (clienteModoModificacion) _clienteCargarEdicion();

    // Habilitar botón asignar usuario
    var btnAsig = document.getElementById('btn-asignar-usuario-cli');
    if (btnAsig) btnAsig.disabled = false;
}

function clienteModoEdicion() {
    if (!clienteSeleccionadoId) { alert('Seleccione un cliente de la tabla.'); return; }
    clienteModoModificacion = true;
    _clienteCargarEdicion();
    document.getElementById('btn-cli-guardar').textContent = 'ACTUALIZAR';
    document.getElementById('btn-cli-guardar').style.background = '#27ae60';
}

function _clienteCargarEdicion() {
    document.getElementById('h-cli-edit-id').value = clienteSeleccionadoId;
    document.getElementById('h-cli-edit-doc').value = document.getElementById('cli-doc').value;
    document.getElementById('h-cli-edit-nombre').value = document.getElementById('cli-nombre').value;
    document.getElementById('h-cli-edit-apellido').value = document.getElementById('cli-apellido').value;
    document.getElementById('h-cli-edit-dir').value = document.getElementById('cli-dir').value;
}

function clienteGuardarOActualizar() {
    var doc = document.getElementById('cli-doc').value.trim();
    var nom = document.getElementById('cli-nombre').value.trim();
    var ape = document.getElementById('cli-apellido').value.trim();
    if (!doc || doc.length !== 8) { alert('El documento debe tener 8 dígitos.'); return; }
    if (!nom) { alert('Ingrese el nombre.'); return; }
    if (!ape) { alert('Ingrese el apellido.'); return; }

    if (clienteModoModificacion) {
        if (!clienteSeleccionadoId) { alert('Seleccione un cliente.'); return; }
        // Actualizar hidden del form editar
        document.getElementById('h-cli-edit-id').value = clienteSeleccionadoId;
        document.getElementById('h-cli-edit-doc').value = doc;
        document.getElementById('h-cli-edit-nombre').value = nom;
        document.getElementById('h-cli-edit-apellido').value = ape;
        document.getElementById('h-cli-edit-dir').value = document.getElementById('cli-dir').value;
        document.getElementById('form-cliente-editar').submit();
    } else {
        document.getElementById('h-cli-doc').value = doc;
        document.getElementById('h-cli-nombre').value = nom;
        document.getElementById('h-cli-apellido').value = ape;
        document.getElementById('h-cli-dir').value = document.getElementById('cli-dir').value;
        document.getElementById('form-cliente-crear').submit();
    }
}

function clienteEliminar() {
    if (!clienteSeleccionadoId) { alert('Seleccione un cliente.'); return; }
    if (!confirm('¿Eliminar cliente?')) return;
    document.getElementById('h-cli-id').value = clienteSeleccionadoId;
    document.getElementById('form-cliente-eliminar').submit();
}

// Resetear modo al cargar
function _clienteResetearModo() {
    clienteModoModificacion = false;
    clienteSeleccionadoId = null;
    document.getElementById('btn-cli-guardar').textContent = 'GUARDAR';
    document.getElementById('btn-cli-guardar').style.background = '';
}

// ═══════════════════════════════════════════════
// PRODUCTOS — modo guardar / actualizar unificado
// ═══════════════════════════════════════════════
var productoSeleccionadoId = null;
var productoModoModificacion = false;

function toggleFechaVenc(chk) {
    document.getElementById('div-fecha-venc').style.display = chk.checked ? '' : 'none';
}

function seleccionarProducto(row, id, nombre, codigo, precio, stock, esPerecedero, fechaVenc) {
    document.querySelectorAll('#tabla-productos tbody tr').forEach(r => r.classList.remove('selected'));
    row.classList.add('selected');
    productoSeleccionadoId = id;
    document.getElementById('prod-nombre').value = nombre;
    document.getElementById('prod-codigo').value = codigo;
    document.getElementById('prod-precio').value = precio;
    document.getElementById('prod-stock').value = stock;
    var esPerec = esPerecedero === 'True' || esPerecedero === 'true';
    document.getElementById('prod-perecedero').checked = esPerec;
    document.getElementById('div-fecha-venc').style.display = esPerec ? '' : 'none';
    if (fechaVenc) document.getElementById('prod-fecha-venc').value = fechaVenc;
    document.getElementById('h-prod-del-id').value = id;
}

function productoModoEdicion() {
    if (!productoSeleccionadoId) { alert('Seleccione un producto.'); return; }
    productoModoModificacion = true;
    document.getElementById('btn-prod-guardar').textContent = 'ACTUALIZAR';
    document.getElementById('btn-prod-guardar').style.background = '#27ae60';
}

function productoGuardarOActualizar() {
    var nom = document.getElementById('prod-nombre').value.trim();
    var cod = document.getElementById('prod-codigo').value.trim();
    var precio = document.getElementById('prod-precio').value;
    var perec = document.getElementById('prod-perecedero').checked;
    var fvenc = document.getElementById('prod-fecha-venc').value;
    if (!nom) { alert('Ingrese el nombre.'); return; }
    if (!cod || cod.length !== 8) { alert('El código debe tener 8 dígitos.'); return; }
    if (!precio || parseFloat(precio) <= 0) { alert('Precio inválido.'); return; }
    if (perec && !fvenc) { alert('Ingrese la fecha de vencimiento.'); return; }

    if (productoModoModificacion) {
        if (!productoSeleccionadoId) { alert('Seleccione un producto.'); return; }
        document.getElementById('h-prod-edit-id').value = productoSeleccionadoId;
        document.getElementById('h-prod-edit-nombre').value = nom;
        document.getElementById('h-prod-edit-codigo').value = cod;
        document.getElementById('h-prod-edit-precio').value = precio;
        document.getElementById('h-prod-edit-stock').value = document.getElementById('prod-stock').value || '0';
        document.getElementById('h-prod-edit-perecedero').value = perec ? 'true' : 'false';
        document.getElementById('h-prod-edit-fecha-venc').value = fvenc || '';
        document.getElementById('form-prod-editar').submit();
    } else {
        document.getElementById('h-prod-nombre').value = nom;
        document.getElementById('h-prod-codigo').value = cod;
        document.getElementById('h-prod-precio').value = precio;
        document.getElementById('h-prod-stock').value = document.getElementById('prod-stock').value || '0';
        document.getElementById('h-prod-perecedero').value = perec ? 'true' : 'false';
        document.getElementById('h-prod-fecha-venc').value = fvenc || '';
        document.getElementById('form-prod-crear').submit();
    }
}

function productoEliminar() {
    if (!productoSeleccionadoId) { alert('Seleccione un producto.'); return; }
    if (!confirm('¿Eliminar producto?')) return;
    document.getElementById('h-prod-del-id').value = productoSeleccionadoId;
    document.getElementById('form-prod-eliminar').submit();
}

// ═══════════════════════════════════════════════
// PROVEEDORES — modo guardar / actualizar unificado
// ═══════════════════════════════════════════════
var proveedorSeleccionadoId = null;
var proveedorModoModificacion = false;

function seleccionarProveedor(row, id, cuit, razon, tel, email, dir) {
    document.querySelectorAll('#tabla-proveedores tbody tr').forEach(r => r.classList.remove('selected'));
    row.classList.add('selected');
    proveedorSeleccionadoId = id;
    document.getElementById('prov-cuit').value = cuit;
    document.getElementById('prov-razon').value = razon;
    document.getElementById('prov-tel').value = tel;
    document.getElementById('prov-email').value = email;
    document.getElementById('prov-dir').value = dir;
    document.getElementById('h-prov-del-id').value = id;

    // Habilitar botones
    var btnAsig = document.getElementById('btn-asignar-usuario-prov');
    if (btnAsig) btnAsig.disabled = false;
    var btnAsigProd = document.getElementById('btn-asignar-productos-prov');
    if (btnAsigProd) btnAsigProd.disabled = false;
    var btnCat = document.getElementById('btn-ver-catalogo-prov');
    if (btnCat) btnCat.disabled = false;
}

function proveedorModoEdicion() {
    if (!proveedorSeleccionadoId) { alert('Seleccione un proveedor.'); return; }
    proveedorModoModificacion = true;
    document.getElementById('btn-prov-guardar').textContent = 'ACTUALIZAR';
    document.getElementById('btn-prov-guardar').style.background = '#27ae60';
}

function proveedorGuardarOActualizar() {
    var cuit = document.getElementById('prov-cuit').value.trim();
    var razon = document.getElementById('prov-razon').value.trim();
    if (!cuit || cuit.length !== 11) { alert('El CUIT debe tener 11 dígitos.'); return; }
    if (!razon) { alert('Ingrese la razón social.'); return; }

    if (proveedorModoModificacion) {
        if (!proveedorSeleccionadoId) { alert('Seleccione un proveedor.'); return; }
        document.getElementById('h-prov-edit-id').value = proveedorSeleccionadoId;
        document.getElementById('h-prov-edit-cuit').value = cuit;
        document.getElementById('h-prov-edit-razon').value = razon;
        document.getElementById('h-prov-edit-tel').value = document.getElementById('prov-tel').value;
        document.getElementById('h-prov-edit-email').value = document.getElementById('prov-email').value;
        document.getElementById('h-prov-edit-dir').value = document.getElementById('prov-dir').value;
        document.getElementById('form-prov-editar').submit();
    } else {
        document.getElementById('h-prov-cuit').value = cuit;
        document.getElementById('h-prov-razon').value = razon;
        document.getElementById('h-prov-tel').value = document.getElementById('prov-tel').value;
        document.getElementById('h-prov-email').value = document.getElementById('prov-email').value;
        document.getElementById('h-prov-dir').value = document.getElementById('prov-dir').value;
        document.getElementById('form-prov-crear').submit();
    }
}

function proveedorEliminar() {
    if (!proveedorSeleccionadoId) { alert('Seleccione un proveedor.'); return; }
    if (!confirm('¿Eliminar proveedor?')) return;
    document.getElementById('h-prov-del-id').value = proveedorSeleccionadoId;
    document.getElementById('form-prov-eliminar').submit();
}

// ═══════════════════════════════════════════════
// NUEVA VENTA
// ═══════════════════════════════════════════════
var ventaDetalles = [];

function ventaAgregarProducto() {
    var sel = document.getElementById('venta-sel-producto');
    var opt = sel.options[sel.selectedIndex];
    var cant = parseInt(document.getElementById('venta-cantidad').value) || 0;
    if (!opt.value) { alert('Seleccione un producto.'); return; }
    if (cant <= 0) { alert('Cantidad inválida.'); return; }
    var stock = parseInt(opt.dataset.stock);
    if (cant > stock) { alert('Stock insuficiente. Disponible: ' + stock); return; }
    var precio = parseFloat(opt.dataset.precio);
    var existing = ventaDetalles.find(d => d.productoId == opt.value);
    if (existing) {
        if (existing.cantidad + cant > stock) { alert('Stock insuficiente.'); return; }
        existing.cantidad += cant; existing.subtotal = existing.cantidad * precio;
    } else {
        ventaDetalles.push({ productoId: opt.value, nombre: opt.dataset.nombre, cantidad: cant, precio: precio, subtotal: cant * precio });
    }
    ventaRenderTabla();
}
function ventaEliminarDetalle(idx) { ventaDetalles.splice(idx, 1); ventaRenderTabla(); }
function ventaRenderTabla() {
    var tbody = document.getElementById('venta-tbody'), total = 0;
    if (!ventaDetalles.length) {
        tbody.innerHTML = '<tr id="venta-empty-row"><td colspan="5" style="text-align:center;color:#aaa;padding:12px;">Agregue productos...</td></tr>';
        document.getElementById('venta-total').textContent = '0.00';
        document.getElementById('btn-finalizar-venta').disabled = true;
        document.getElementById('venta-hidden-inputs').innerHTML = '';
        return;
    }
    tbody.innerHTML = '';
    ventaDetalles.forEach(function (d, i) {
        total += d.subtotal;
        tbody.innerHTML += '<tr><td>' + d.nombre + '</td><td>' + d.cantidad + '</td><td>$' + d.precio.toFixed(2) + '</td><td>$' + d.subtotal.toFixed(2) + '</td><td><button type="button" class="btn-form-del" style="padding:2px 8px" onclick="ventaEliminarDetalle(' + i + ')">X</button></td></tr>';
    });
    document.getElementById('venta-total').textContent = total.toFixed(2);
    document.getElementById('btn-finalizar-venta').disabled = false;
    var hidden = '';
    ventaDetalles.forEach(function (d, i) { hidden += '<input type="hidden" name="Detalles[' + i + '].ProductoId" value="' + d.productoId + '"/><input type="hidden" name="Detalles[' + i + '].ProductoNombre" value="' + d.nombre + '"/><input type="hidden" name="Detalles[' + i + '].Cantidad" value="' + d.cantidad + '"/><input type="hidden" name="Detalles[' + i + '].PrecioUnitario" value="' + d.precio + '"/>'; });
    document.getElementById('venta-hidden-inputs').innerHTML = hidden;
}
function ventaLimpiar() { ventaDetalles = []; ventaRenderTabla(); }

// ═══════════════════════════════════════════════
// NUEVA COMPRA
// ═══════════════════════════════════════════════
var compraDetalles = [];

function compraCargarProductos() {
    var provId = document.getElementById('compra-proveedor').value;
    if (!provId) return;
    fetch('/Compra/ObtenerProductosProveedor?proveedorId=' + provId)
        .then(r => r.json())
        .then(data => {
            var sel = document.getElementById('compra-sel-producto');
            sel.innerHTML = '<option value="">— Seleccione —</option>';
            data.forEach(p => { sel.innerHTML += '<option value="' + p.id + '" data-precio="' + p.precio + '" data-nombre="' + p.nombre + '" data-stock="' + p.stock + '">' + p.nombre + ' (Stock: ' + p.stock + ')</option>'; });
        });
}
function compraAgregarProducto() {
    var sel = document.getElementById('compra-sel-producto');
    var opt = sel.options[sel.selectedIndex];
    var cant = parseInt(document.getElementById('compra-cantidad').value) || 0;
    var precio = parseFloat(document.getElementById('compra-precio').value) || 0;
    if (!opt.value) { alert('Seleccione producto.'); return; }
    if (cant <= 0) { alert('Cantidad inválida.'); return; }
    if (precio <= 0) { alert('Precio inválido.'); return; }
    compraDetalles.push({ productoId: opt.value, nombre: opt.dataset.nombre, cantidad: cant, precio: precio, subtotal: cant * precio });
    compraRenderTabla();
    document.getElementById('compra-precio').value = '';
    document.getElementById('compra-cantidad').value = 1;
}
function compraEliminarDetalle(idx) { compraDetalles.splice(idx, 1); compraRenderTabla(); }
function compraRenderTabla() {
    var tbody = document.getElementById('compra-tbody'), total = 0;
    if (!compraDetalles.length) {
        tbody.innerHTML = '<tr id="compra-empty-row"><td colspan="5" style="text-align:center;color:#aaa;padding:12px;">Seleccione proveedor...</td></tr>';
        document.getElementById('compra-total').textContent = '0.00';
        document.getElementById('btn-finalizar-compra').disabled = true;
        document.getElementById('compra-hidden-inputs').innerHTML = '';
        return;
    }
    tbody.innerHTML = '';
    compraDetalles.forEach(function (d, i) {
        total += d.subtotal;
        tbody.innerHTML += '<tr><td>' + d.nombre + '</td><td>' + d.cantidad + '</td><td>$' + d.precio.toFixed(2) + '</td><td>$' + d.subtotal.toFixed(2) + '</td><td><button type="button" class="btn-form-del" style="padding:2px 8px" onclick="compraEliminarDetalle(' + i + ')">X</button></td></tr>';
    });
    document.getElementById('compra-total').textContent = total.toFixed(2);
    document.getElementById('btn-finalizar-compra').disabled = false;
    var hidden = '';
    compraDetalles.forEach(function (d, i) { hidden += '<input type="hidden" name="Detalles[' + i + '].ProductoId" value="' + d.productoId + '"/><input type="hidden" name="Detalles[' + i + '].ProductoNombre" value="' + d.nombre + '"/><input type="hidden" name="Detalles[' + i + '].Cantidad" value="' + d.cantidad + '"/><input type="hidden" name="Detalles[' + i + '].PrecioUnitario" value="' + d.precio + '"/>'; });
    document.getElementById('compra-hidden-inputs').innerHTML = hidden;
}
function compraLimpiar() {
    compraDetalles = []; compraRenderTabla();
    document.getElementById('compra-proveedor').selectedIndex = 0;
    document.getElementById('compra-sel-producto').innerHTML = '<option value="">— Seleccione proveedor primero —</option>';
    document.getElementById('compra-factura').value = '';
    document.getElementById('compra-precio').value = '';
    document.getElementById('compra-cantidad').value = 1;
}

// ═══════════════════════════════════════════════
// VENTAS TOTALES — acciones
// ═══════════════════════════════════════════════
var vtSeleccionadoId = null, vtSeleccionadaFila = null;

function ventaSeleccionarFila(row, id) {
    document.querySelectorAll('#tabla-ventas tbody tr').forEach(r => r.classList.remove('selected'));
    row.classList.add('selected');
    vtSeleccionadoId = id; vtSeleccionadaFila = row;
    document.getElementById('btn-vt-detalle').disabled = false;
    document.getElementById('btn-vt-eliminar').disabled = false;
}
function ventaVerDetalle() {
    if (!vtSeleccionadaFila) { alert('Seleccione una venta.'); return; }
    var r = vtSeleccionadaFila;
    var detalles = JSON.parse(r.dataset.detalles.replace(/&quot;/g, '"'));
    var txt = 'DETALLE DE VENTA - ID: ' + r.dataset.id + '\nFecha:         ' + r.dataset.fecha + '\nCliente:       ' + r.dataset.cliente + '\nForma de Pago: ' + r.dataset.formapago + '\n\nPRODUCTOS:\n------------------------------------------------\n';
    detalles.forEach(d => { txt += '- ' + d.nombre + '\n  Cantidad:        ' + d.cantidad + '\n  Precio Unitario: $' + d.precio + '\n  Subtotal:        $' + d.subtotal + '\n\n'; });
    txt += '------------------------------------------------\nTOTAL: $' + r.dataset.total;
    abrirModal('Detalle de Venta', txt);
}
function ventaEliminarSeleccionada() {
    if (!vtSeleccionadoId) { alert('Seleccione una venta.'); return; }
    if (!confirm('¿Eliminar venta #' + vtSeleccionadoId + '?\n\nSe restaurará el stock. Esta acción no se puede deshacer.')) return;
    document.getElementById('vt-eliminar-id').value = vtSeleccionadoId;
    document.getElementById('form-vt-eliminar').submit();
}
function ventaMostrarEstadisticas() {
    var rows = Array.from(document.querySelectorAll('#tabla-ventas tbody tr')).filter(r => r.style.display !== 'none' && r.dataset.id);
    if (!rows.length) { alert('No hay ventas visibles.'); return; }
    var totalGen = 0, detallesAll = [], fpMap = {};
    rows.forEach(r => {
        var total = parseFloat(r.dataset.total.replace(',', ''));
        totalGen += total;
        var fp = r.dataset.formapago;
        if (!fpMap[fp]) fpMap[fp] = { total: 0, cantidad: 0 };
        fpMap[fp].total += total; fpMap[fp].cantidad++;
        try { JSON.parse(r.dataset.detalles.replace(/&quot;/g, '"')).forEach(d => detallesAll.push(d)); } catch (e) { }
    });
    var cant = rows.length;
    var txt = 'ESTADÍSTICAS DE VENTAS\n------------------------------------------------\nTotal ventas:       $' + totalGen.toFixed(2) + '\nCantidad de ventas:  ' + cant + '\nPromedio por venta: $' + (totalGen / cant).toFixed(2) + '\n\nVENTAS POR FORMA DE PAGO:\n';
    Object.keys(fpMap).forEach(fp => { txt += fp + ':\n  Total:    $' + fpMap[fp].total.toFixed(2) + '\n  Cantidad:  ' + fpMap[fp].cantidad + '\n'; });
    var prodMap = {};
    detallesAll.forEach(d => { if (!prodMap[d.nombre]) prodMap[d.nombre] = { cantidad: 0, total: 0 }; prodMap[d.nombre].cantidad += parseInt(d.cantidad) || 0; prodMap[d.nombre].total += parseFloat(d.subtotal.replace(',', '')) || 0; });
    txt += '\nPRODUCTOS MÁS VENDIDOS:\n';
    Object.keys(prodMap).map(k => ({ nombre: k, ...prodMap[k] })).sort((a, b) => b.cantidad - a.cantidad).slice(0, 5).forEach(p => { txt += p.nombre + ':\n  Cantidad: ' + p.cantidad + '\n  Total:   $' + p.total.toFixed(2) + '\n'; });
    abrirModal('Estadísticas de Ventas', txt);
}
function ventaGenerarInforme() {
    var desde = document.getElementById('vt-desde').value;
    var hasta = document.getElementById('vt-hasta').value;
    var url = '/Reporte/GenerarReporteVentas';
    if (desde) url += '?desde=' + desde;
    if (hasta) url += (desde ? '&' : '?') + 'hasta=' + hasta;
    window.open(url, '_blank');
}
function filtrarVentasFecha() {
    var desde = document.getElementById('vt-desde').value;
    var hasta = document.getElementById('vt-hasta').value;
    document.querySelectorAll('#tabla-ventas tbody tr').forEach(function (row) {
        if (!row.dataset.fecha) return;
        var p = row.dataset.fecha.split(' ')[0].split('/');
        var f = p[2] + '-' + p[1] + '-' + p[0];
        row.style.display = ((!desde || f >= desde) && (!hasta || f <= hasta)) ? '' : 'none';
    });
    recalcTotalVentas();
}
function limpiarFiltroVentas() {
    document.getElementById('vt-desde').value = '';
    document.getElementById('vt-hasta').value = '';
    document.getElementById('search-ventas').value = '';
    document.querySelectorAll('#tabla-ventas tbody tr').forEach(r => r.style.display = '');
    recalcTotalVentas();
}
function recalcTotalVentas() {
    var total = 0;
    document.querySelectorAll('#tabla-ventas tbody tr').forEach(r => {
        if (r.style.display === 'none' || !r.dataset.total) return;
        total += parseFloat(r.dataset.total.replace(',', '')) || 0;
    });
    var lbl = document.getElementById('vt-total-label');
    if (lbl) lbl.textContent = total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}

// ═══════════════════════════════════════════════
// COMPRAS TOTALES — acciones
// ═══════════════════════════════════════════════
var ctSeleccionadoId = null, ctSeleccionadaFila = null;

function compraSeleccionarFila(row, id) {
    document.querySelectorAll('#tabla-compras tbody tr').forEach(r => r.classList.remove('selected'));
    row.classList.add('selected');
    ctSeleccionadoId = id; ctSeleccionadaFila = row;
    document.getElementById('btn-ct-detalle').disabled = false;
    document.getElementById('btn-ct-eliminar').disabled = false;
}
function compraVerDetalle() {
    if (!ctSeleccionadaFila) { alert('Seleccione una compra.'); return; }
    var r = ctSeleccionadaFila;
    var detalles = JSON.parse(r.dataset.detalles.replace(/&quot;/g, '"'));
    var txt = 'DETALLE DE COMPRA - ID: ' + r.dataset.id + '\nFecha:          ' + r.dataset.fecha + '\nProveedor:      ' + r.dataset.proveedor + '\nN° Factura:     ' + r.dataset.factura + '\nForma de Pago:  ' + r.dataset.formapago + '\n\nPRODUCTOS:\n------------------------------------------------\n';
    detalles.forEach(d => { txt += '- ' + d.nombre + '\n  Cantidad:        ' + d.cantidad + '\n  Precio Unitario: $' + d.precio + '\n  Subtotal:        $' + d.subtotal + '\n\n'; });
    txt += '------------------------------------------------\nTOTAL: $' + r.dataset.total;
    abrirModal('Detalle de Compra', txt);
}
function compraEliminarSeleccionada() {
    if (!ctSeleccionadoId) { alert('Seleccione una compra.'); return; }
    if (!confirm('¿Eliminar compra #' + ctSeleccionadoId + '?\n\nSe restaurará el stock. Esta acción no se puede deshacer.')) return;
    document.getElementById('ct-eliminar-id').value = ctSeleccionadoId;
    document.getElementById('form-ct-eliminar').submit();
}
function compraMostrarEstadisticas() {
    var rows = Array.from(document.querySelectorAll('#tabla-compras tbody tr')).filter(r => r.style.display !== 'none' && r.dataset.id);
    if (!rows.length) { alert('No hay compras visibles.'); return; }
    var totalGen = 0, fpMap = {}, provMap = {};
    rows.forEach(r => {
        var total = parseFloat(r.dataset.total.replace(',', ''));
        totalGen += total;
        var fp = r.dataset.formapago;
        if (!fpMap[fp]) fpMap[fp] = { total: 0, cantidad: 0 };
        fpMap[fp].total += total; fpMap[fp].cantidad++;
        var prov = r.dataset.proveedor || '—';
        if (!provMap[prov]) provMap[prov] = { total: 0, cantidad: 0 };
        provMap[prov].total += total; provMap[prov].cantidad++;
    });
    var cant = rows.length;
    var txt = 'ESTADÍSTICAS DE COMPRAS\n------------------------------------------------\nTotal compras:       $' + totalGen.toFixed(2) + '\nCantidad de compras:  ' + cant + '\nPromedio por compra: $' + (totalGen / cant).toFixed(2) + '\n\nCOMPRAS POR FORMA DE PAGO:\n';
    Object.keys(fpMap).forEach(fp => { txt += fp + ':\n  Total:    $' + fpMap[fp].total.toFixed(2) + '\n  Cantidad:  ' + fpMap[fp].cantidad + '\n'; });
    txt += '\nPRINCIPALES PROVEEDORES:\n';
    Object.keys(provMap).map(k => ({ nombre: k, ...provMap[k] })).sort((a, b) => b.total - a.total).slice(0, 5).forEach(p => { txt += p.nombre + ':\n  Total Comprado:   $' + p.total.toFixed(2) + '\n  Cant. de Compras:  ' + p.cantidad + '\n'; });
    abrirModal('Estadísticas de Compras', txt);
}
function compraGenerarInforme() {
    var desde = document.getElementById('ct-desde').value;
    var hasta = document.getElementById('ct-hasta').value;
    var url = '/Reporte/GenerarReporteCompras';
    if (desde) url += '?desde=' + desde;
    if (hasta) url += (desde ? '&' : '?') + 'hasta=' + hasta;
    window.open(url, '_blank');
}
function filtrarComprasFecha() {
    var desde = document.getElementById('ct-desde').value;
    var hasta = document.getElementById('ct-hasta').value;
    document.querySelectorAll('#tabla-compras tbody tr').forEach(function (row) {
        if (!row.dataset.fecha) return;
        var p = row.dataset.fecha.split(' ')[0].split('/');
        var f = p[2] + '-' + p[1] + '-' + p[0];
        row.style.display = ((!desde || f >= desde) && (!hasta || f <= hasta)) ? '' : 'none';
    });
    recalcTotalCompras();
}
function limpiarFiltroCompras() {
    document.getElementById('ct-desde').value = '';
    document.getElementById('ct-hasta').value = '';
    document.getElementById('search-compras').value = '';
    document.querySelectorAll('#tabla-compras tbody tr').forEach(r => r.style.display = '');
    recalcTotalCompras();
}
function recalcTotalCompras() {
    var total = 0;
    document.querySelectorAll('#tabla-compras tbody tr').forEach(r => {
        if (r.style.display === 'none' || !r.dataset.total) return;
        total += parseFloat(r.dataset.total.replace(',', '')) || 0;
    });
    var lbl = document.getElementById('ct-total-label');
    if (lbl) lbl.textContent = total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}

// ═══════════════════════════════════════════════
// SEGURIDAD — sub-secciones
// ═══════════════════════════════════════════════
function mostrarSeccion(id) {
    document.querySelectorAll('.seg-seccion').forEach(s => s.style.display = 'none');
    document.querySelector('.seg-menu').style.display = 'none';
    var sec = document.getElementById(id);
    if (sec) sec.style.display = 'block';
    // Cargar backups si es esa sección
    if (id === 'seg-backup') backupCargarLista();
}
function ocultarSecciones() {
    document.querySelectorAll('.seg-seccion').forEach(s => s.style.display = 'none');
    var menu = document.querySelector('.seg-menu');
    if (menu) menu.style.display = '';
}

// ═══════════════════════════════════════════════
// AUDITORÍA — filtro inline (sin redirigir)
// ═══════════════════════════════════════════════
function auditoriaFiltrar() {
    var usuarioId = document.getElementById('aud-usuario-sel').value;
    var desde = document.getElementById('aud-desde').value;
    var hasta = document.getElementById('aud-hasta').value;

    var params = new URLSearchParams();
    if (usuarioId) params.append('usuarioId', usuarioId);
    if (desde) params.append('desde', desde);
    if (hasta) params.append('hasta', hasta);

    fetch('/Auditoria/Filtrar?' + params.toString())
        .then(r => r.json())
        .then(data => {
            var tbody = document.getElementById('aud-tbody');
            tbody.innerHTML = '';
            if (!data.length) {
                tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:#aaa;padding:16px;">Sin registros para esos filtros.</td></tr>';
            } else {
                data.forEach(function (a) {
                    var badge = a.sesionActiva
                        ? '<span class="aud-badge activa">● Activa</span>'
                        : '<span class="aud-badge cerrada">○ Cerrada</span>';
                    tbody.innerHTML += '<tr><td><strong>' + a.nombreUsuario + '</strong></td><td>' + a.fechaIngreso + '</td><td>' + a.fechaSalida + '</td><td>' + a.direccionIP + '</td><td>' + a.dispositivo + '</td><td>' + a.tipoSesion + '</td><td>' + badge + '</td></tr>';
                });
            }
            // Actualizar contadores
            document.getElementById('aud-total-count').textContent = data.length;
            document.getElementById('aud-activa-count').textContent = data.filter(a => a.sesionActiva).length;
            document.getElementById('aud-cerrada-count').textContent = data.filter(a => !a.sesionActiva).length;
        })
        .catch(() => alert('Error al filtrar auditoría.'));
}
function auditoriaLimpiar() {
    document.getElementById('aud-usuario-sel').value = '';
    document.getElementById('aud-desde').value = '';
    document.getElementById('aud-hasta').value = '';
    auditoriaFiltrar(); // Recarga todo
}

// ═══════════════════════════════════════════════
// BACKUP
// ═══════════════════════════════════════════════
function backupCargarLista() {
    fetch('/Backup/ListarBackups')
        .then(r => r.json())
        .then(data => {
            var sel = document.getElementById('sel-backups');
            sel.innerHTML = '<option value="">— Seleccione un backup —</option>';
            if (data.ok && data.backups.length) {
                data.backups.forEach(b => {
                    sel.innerHTML += '<option value="' + b.nombre + '">' + b.nombre + ' (' + b.tamaño + ') — ' + b.fecha + '</option>';
                });
                document.getElementById('lbl-ultimo-backup').textContent = data.backups[0].fecha;
            } else {
                sel.innerHTML = '<option value="">Sin backups disponibles</option>';
                document.getElementById('lbl-ultimo-backup').textContent = 'No hay backups';
            }
        })
        .catch(() => {
            document.getElementById('sel-backups').innerHTML = '<option value="">Error al cargar</option>';
        });
}

function backupCrear() {
    var btn = document.getElementById('btn-crear-backup');
    var status = document.getElementById('backup-status');
    btn.disabled = true;
    btn.textContent = 'Creando backup...';
    status.style.display = 'none';

    fetch('/Backup/CrearBackup', { method: 'POST', headers: { 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || '' } })
        .then(r => r.json())
        .then(data => {
            btn.disabled = false;
            btn.textContent = 'CREAR BACKUP AHORA';
            if (data.ok) {
                status.style.color = '#27ae60';
                status.textContent = '✓ ' + data.mensaje;
                status.style.display = 'block';
                backupCargarLista();
            } else {
                status.style.color = '#c0392b';
                status.textContent = '✗ ' + data.error;
                status.style.display = 'block';
            }
        })
        .catch(err => {
            btn.disabled = false;
            btn.textContent = 'CREAR BACKUP AHORA';
            status.style.color = '#c0392b';
            status.textContent = '✗ Error de conexión.';
            status.style.display = 'block';
        });
}

function backupDescargar() {
    var nombre = document.getElementById('sel-backups').value;
    if (!nombre) { alert('Seleccione un backup.'); return; }
    window.location.href = '/Backup/DescargarBackup?nombreArchivo=' + encodeURIComponent(nombre);
}

function backupRestaurar() {
    var nombre = document.getElementById('sel-backups').value;
    if (!nombre) { alert('Seleccione un backup.'); return; }
    if (!confirm('¿Restaurar la base de datos con "' + nombre + '"?\n\nIMPORTANTE: Se cerrarán todas las conexiones activas.\nEsta acción no se puede deshacer.')) return;

    var status = document.getElementById('restore-status');
    status.style.color = '#2980b9';
    status.textContent = 'Restaurando... esto puede tardar varios minutos.';
    status.style.display = 'block';

    fetch('/Backup/RestaurarBackup', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nombreArchivo: nombre })
    })
        .then(r => r.json())
        .then(data => {
            if (data.ok) {
                status.style.color = '#27ae60';
                status.textContent = '✓ ' + data.mensaje + ' Se recomienda reiniciar la aplicación.';
            } else {
                status.style.color = '#c0392b';
                status.textContent = '✗ ' + data.error;
            }
        })
        .catch(() => {
            status.style.color = '#c0392b';
            status.textContent = '✗ Error de conexión durante la restauración.';
        });
}

// ═══════════════════════════════════════════════
// INICIALIZACIÓN
// ═══════════════════════════════════════════════
document.addEventListener('DOMContentLoaded', function () {
    // Fechas por defecto (últimos 30 días)
    var hoy = new Date(), hace30 = new Date();
    hace30.setDate(hoy.getDate() - 30);
    function fmt(d) { return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0'); }
    ['vt-desde', 'ct-desde'].forEach(id => { var el = document.getElementById(id); if (el) el.value = fmt(hace30); });
    ['vt-hasta', 'ct-hasta'].forEach(id => { var el = document.getElementById(id); if (el) el.value = fmt(hoy); });
});
// ═══════════════════════════════════════════════
// SEGURIDAD — GRUPOS
// ═══════════════════════════════════════════════
var sgGrupoSeleccionadoId = null;

function sgSeleccionarGrupo(row, id) {
    document.querySelectorAll('#tabla-seg-grupos tbody tr').forEach(r => r.classList.remove('selected'));
    row.classList.add('selected');
    sgGrupoSeleccionadoId = id;
    var btn1 = document.getElementById('btn-sg-grp-eliminar');
    var btn2 = document.getElementById('btn-sg-grp-modificar');
    if (btn1) btn1.disabled = false;
    if (btn2) btn2.disabled = false;
}

function sgFiltrarGrupos() {
    var nombre = (document.getElementById('sg-grp-fnombre')?.value || '').toLowerCase();
    var estado = (document.getElementById('sg-grp-festado')?.value || '').toLowerCase();
    document.querySelectorAll('#tabla-seg-grupos tbody tr').forEach(function (row) {
        var nom = (row.cells[1]?.textContent || '').toLowerCase();
        var est = (row.cells[4]?.textContent || '').toLowerCase();
        var ok = (!nombre || nom.includes(nombre)) && (!estado || est.includes(estado));
        row.style.display = ok ? '' : 'none';
    });
}

function sgGruposRefrescar() {
    document.getElementById('sg-grp-fnombre').value = '';
    document.getElementById('sg-grp-festado').value = '';
    sgFiltrarGrupos();
    sgGrupoSeleccionadoId = null;
    var btn1 = document.getElementById('btn-sg-grp-eliminar');
    var btn2 = document.getElementById('btn-sg-grp-modificar');
    if (btn1) btn1.disabled = true;
    if (btn2) btn2.disabled = true;
    document.querySelectorAll('#tabla-seg-grupos tbody tr').forEach(r => r.classList.remove('selected'));
}

function abrirModalAgregarGrupo() {
    document.getElementById('mg-id').value = '';
    document.getElementById('mg-nombre').value = '';
    document.getElementById('mg-codigo').value = '';
    document.getElementById('mg-desc').value = '';
    document.getElementById('modal-grp-titulo').textContent = 'AGREGAR GRUPO';
    document.getElementById('modal-grupo-overlay').style.display = 'flex';
    // Configurar el form para crear
    document.getElementById('form-modal-grupo').action = '/Grupo/Crear';
}

function abrirModalModificarGrupo() {
    if (!sgGrupoSeleccionadoId) { alert('Seleccione un grupo.'); return; }
    var row = document.querySelector('#tabla-seg-grupos tbody tr.selected');
    if (!row) return;
    document.getElementById('mg-id').value = sgGrupoSeleccionadoId;
    document.getElementById('mg-nombre').value = row.dataset.nombre || '';
    document.getElementById('mg-codigo').value = row.cells[2]?.textContent || '';
    document.getElementById('mg-desc').value = row.dataset.descripcion || '';
    document.getElementById('modal-grp-titulo').textContent = 'MODIFICAR GRUPO';
    document.getElementById('form-modal-grupo').action = '/Grupo/Editar';
    document.getElementById('modal-grupo-overlay').style.display = 'flex';
}

function cerrarModalGrupo() {
    document.getElementById('modal-grupo-overlay').style.display = 'none';
}

function guardarGrupo() {
    var nombre = document.getElementById('mg-nombre').value.trim();
    if (!nombre) { alert('Ingrese el nombre del grupo.'); return; }
    document.getElementById('form-modal-grupo').submit();
}

function sgEliminarGrupo() {
    if (!sgGrupoSeleccionadoId) { alert('Seleccione un grupo.'); return; }
    var row = document.querySelector('#tabla-seg-grupos tbody tr.selected');
    var nombre = row ? row.cells[1]?.textContent : 'el grupo';
    if (!confirm('¿Eliminar el grupo "' + nombre + '"?')) return;
    // Crear form temporal y enviar
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Grupo/Eliminar';
    var input = document.createElement('input');
    input.type = 'hidden'; input.name = 'id'; input.value = sgGrupoSeleccionadoId;
    form.appendChild(input);
    // Token CSRF si existe
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (token) {
        var t2 = document.createElement('input');
        t2.type = 'hidden'; t2.name = '__RequestVerificationToken'; t2.value = token.value;
        form.appendChild(t2);
    }
    document.body.appendChild(form);
    form.submit();
}

// ═══════════════════════════════════════════════
// SEGURIDAD — USUARIOS
// ═══════════════════════════════════════════════
var sgUsuarioSeleccionadoId = null;

function sgSeleccionarUsuario(row, id) {
    document.querySelectorAll('#tabla-seg-usuarios tbody tr').forEach(r => r.classList.remove('selected'));
    row.classList.add('selected');
    sgUsuarioSeleccionadoId = id;
    var btns = ['btn-sg-usr-eliminar', 'btn-sg-usr-modificar', 'btn-sg-usr-resetear'];
    btns.forEach(b => { var el = document.getElementById(b); if (el) el.disabled = false; });
}

function sgFiltrarUsuarios() {
    var nombre = (document.getElementById('sg-usr-fnombre')?.value || '').toLowerCase();
    var grupo = (document.getElementById('sg-usr-fgrupo')?.value || '').toLowerCase();
    var estado = (document.getElementById('sg-usr-festado')?.value || '').toLowerCase();
    document.querySelectorAll('#tabla-seg-usuarios tbody tr').forEach(function (row) {
        var nom = (row.cells[0]?.textContent || '').toLowerCase();
        var grupos = (row.dataset.grupos || '').toLowerCase();
        var activo = row.cells[2]?.querySelector('span')?.textContent.includes('✓') ? 'activo' : 'inactivo';
        var ok = (!nombre || nom.includes(nombre)) &&
            (!grupo || grupos.includes(grupo)) &&
            (!estado || activo.includes(estado.toLowerCase()));
        row.style.display = ok ? '' : 'none';
    });
}

function sgUsuariosRefrescar() {
    ['sg-usr-fnombre', 'sg-usr-fgrupo', 'sg-usr-festado'].forEach(id => {
        var el = document.getElementById(id); if (el) el.value = '';
    });
    sgFiltrarUsuarios();
    sgUsuarioSeleccionadoId = null;
    ['btn-sg-usr-eliminar', 'btn-sg-usr-modificar', 'btn-sg-usr-resetear'].forEach(b => {
        var el = document.getElementById(b); if (el) el.disabled = true;
    });
    document.querySelectorAll('#tabla-seg-usuarios tbody tr').forEach(r => r.classList.remove('selected'));
}

function sgAgregarUsuario() {
    document.getElementById('reg-tipo-usuario').value = 'Cliente';
    document.getElementById('reg-campos-cliente').style.display = '';
    document.getElementById('reg-campos-proveedor').style.display = 'none';
    document.getElementById('reg-titulo-datos').textContent = 'DATOS DEL CLIENTE';
    document.getElementById('modal-reg-titulo').textContent = 'REGISTRAR USUARIO';
    document.getElementById('modal-registro-usuario').style.display = 'flex';
}

function sgEliminarUsuario() {
    if (!sgUsuarioSeleccionadoId) { alert('Seleccione un usuario.'); return; }
    var row = document.querySelector('#tabla-seg-usuarios tbody tr.selected');
    var nombre = row ? row.cells[0]?.textContent : 'el usuario';
    if (nombre === 'admin') { alert('No se puede eliminar el usuario administrador.'); return; }
    if (!confirm('¿Eliminar el usuario "' + nombre + '"?')) return;
    document.getElementById('sg-usr-del-id').value = sgUsuarioSeleccionadoId;
    document.getElementById('form-sg-usr-eliminar').submit();
}

function sgModificarUsuario() {
    if (!sgUsuarioSeleccionadoId) { alert('Seleccione un usuario.'); return; }
    var row = document.querySelector('#tabla-seg-usuarios tbody tr.selected');
    if (!row) return;

    var nombre = row.cells[0]?.textContent?.trim() || '';
    var emailCell = row.cells[6]?.textContent?.trim() || '';
    var estadoCell = row.cells[2]?.textContent?.trim() || '';

    document.getElementById('mod-usr-id').value = sgUsuarioSeleccionadoId;
    document.getElementById('mod-usr-nombre').textContent = nombre;
    document.getElementById('mod-usr-email').value = emailCell === '—' ? '' : emailCell;
    document.getElementById('mod-usr-estado').value = estadoCell === '✓' ? 'true' : 'false';

    document.getElementById('modal-mod-usuario').style.display = 'flex';
}

function cerrarModalModUsuario() {
    document.getElementById('modal-mod-usuario').style.display = 'none';
}

function sgResetearUsuario() {
    if (!sgUsuarioSeleccionadoId) { alert('Seleccione un usuario.'); return; }
    var row = document.querySelector('#tabla-seg-usuarios tbody tr.selected');
    var nombre = row ? row.cells[0]?.textContent : 'el usuario';
    if (!confirm('¿Resetear la clave del usuario "' + nombre + '"?')) return;
    document.getElementById('sg-usr-reset-id').value = sgUsuarioSeleccionadoId;
    document.getElementById('form-sg-usr-reset').submit();
}

// ═══════════════════════════════════════════════
// SEGURIDAD — GESTIÓN DE SECCIONES
// ═══════════════════════════════════════════════
function mostrarSeccion(id) {
    document.querySelectorAll('.seg-seccion').forEach(s => s.style.display = 'none');
    document.getElementById('seg-menu-principal').style.display = 'none';
    var sec = document.getElementById(id);
    if (sec) sec.style.display = 'block';
    if (id === 'seg-backup') backupCargarLista();
}

function ocultarSecciones() {
    document.querySelectorAll('.seg-seccion').forEach(s => s.style.display = 'none');
    var menu = document.getElementById('seg-menu-principal');
    if (menu) menu.style.display = '';
}