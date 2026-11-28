// Pasa a mayúsculas lo que se escribe en los campos con data-uppercase (por ejemplo la placa)
document.addEventListener('input', function (e) {
    var campo = e.target;
    if (campo && campo.hasAttribute && campo.hasAttribute('data-uppercase')) {
        campo.value = campo.value.toUpperCase();
    }
});

// Confirmación antes de enviar formularios marcados con data-confirmar
document.addEventListener('submit', function (e) {
    var form = e.target;
    var mensaje = form && form.getAttribute ? form.getAttribute('data-confirmar') : null;
    if (mensaje && !window.confirm(mensaje)) {
        e.preventDefault();
    }
});
