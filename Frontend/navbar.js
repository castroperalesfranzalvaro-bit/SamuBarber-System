function renderNavbar(moduloActivo) {
    const rolActual = localStorage.getItem('usuario_rol') || 'Administrador';

    const html = `
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark mb-4 shadow">
        <div class="container-fluid px-4">
            <a class="navbar-brand fw-bold" href="barbero-panel.html">
                <i class="bi bi-scissors me-2 text-warning"></i>SamuBarber System
            </a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav me-auto">
                    <!-- Módulo visible para Barberos y Administradores -->
                    <li class="nav-item">
                        <a class="nav-link ${moduloActivo === 'comisiones' ? 'active fw-bold text-warning' : ''}" href="barbero-panel.html">
                            <i class="bi bi-person-badge me-1"></i> Mi Panel de Comisiones
                        </a>
                    </li>

                    <!-- Módulos EXCLUSIVOS para Administrador -->
                    ${rolActual === 'Administrador' ? `
                    <li class="nav-item">
                        <a class="nav-link ${moduloActivo === 'caja' ? 'active fw-bold text-warning' : ''}" href="cierre-caja.html">
                            <i class="bi bi-cash-stack me-1"></i> Cierre de Caja
                        </a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link ${moduloActivo === 'reportes' ? 'active fw-bold text-warning' : ''}" href="reportes-mensuales.html">
                            <i class="bi bi-bar-chart-line-fill me-1"></i> Reportes Financieros
                        </a>
                    </li>
                    ` : ''}
                </ul>

                <!-- Selector de Perfil / Rol Activo -->
                <div class="d-flex align-items-center text-white gap-2">
                    <span class="badge ${rolActual === 'Administrador' ? 'bg-warning text-dark' : 'bg-info text-dark'} p-2">
                        <i class="bi ${rolActual === 'Administrador' ? 'bi-shield-check' : 'bi-scissors'} me-1"></i>
                        ${rolActual}
                    </span>
                    <select id="globalRolSelect" class="form-select form-select-sm bg-secondary text-white border-0" style="width: auto;" onchange="cambiarRolGlobal(this.value)">
                        <option value="Administrador" ${rolActual === 'Administrador' ? 'selected' : ''}>Rol: Administrador</option>
                        <option value="Barbero" ${rolActual === 'Barbero' ? 'selected' : ''}>Rol: Barbero</option>
                    </select>
                </div>
            </div>
        </div>
    </nav>
    `;

    document.getElementById('navbarContainer').innerHTML = html;
}

function cambiarRolGlobal(nuevoRol) {
    localStorage.setItem('usuario_rol', nuevoRol);
    window.location.reload();
}