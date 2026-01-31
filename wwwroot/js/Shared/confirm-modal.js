// Shared Confirm Modal Helper
// Native confirm() KULLANILMAZ - Bu modal kullanilmalidir

var confirmCallback = null;

function showConfirmModal(options) {
    var modal = document.getElementById('sharedConfirmModal');
    var bsModal = new bootstrap.Modal(modal);

    document.getElementById('confirmModalTitle').textContent = options.title || 'Onay';
    document.getElementById('confirmModalMessage').textContent = options.message || 'Emin misiniz?';

    var btn = document.getElementById('confirmModalBtn');
    var btnText = document.getElementById('confirmModalBtnText');
    var btnIcon = document.getElementById('confirmModalIcon');

    btnText.textContent = options.confirmText || 'Onayla';

    // Buton tipine gore renk
    btn.className = 'btn';
    switch (options.type) {
        case 'danger':
            btn.classList.add('btn-danger');
            break;
        case 'warning':
            btn.classList.add('btn-warning');
            break;
        case 'success':
            btn.classList.add('btn-success');
            break;
        case 'info':
            btn.classList.add('btn-info');
            break;
        default:
            btn.classList.add('btn-primary');
    }

    // Icon
    btnIcon.className = options.confirmIcon || 'bi bi-check';

    confirmCallback = options.onConfirm;

    bsModal.show();
}

function showDeleteConfirm(itemName, onConfirm) {
    showConfirmModal({
        title: 'Silme Onayi',
        message: '"' + itemName + '" kaydini silmek istediginizden emin misiniz? Bu islem geri alinamaz.',
        type: 'danger',
        confirmText: 'Sil',
        confirmIcon: 'bi bi-trash',
        onConfirm: onConfirm
    });
}

// Alias - showConfirm olarak da cagrilabilir
function showConfirm(options) {
    showConfirmModal({
        title: options.title,
        message: options.message,
        type: options.confirmClass === 'btn-danger' ? 'danger' :
              options.confirmClass === 'btn-warning' ? 'warning' :
              options.confirmClass === 'btn-success' ? 'success' : 'primary',
        confirmText: options.confirmText,
        confirmIcon: options.confirmIcon,
        onConfirm: options.onConfirm
    });
}

// Info modal - HTML icerik gostermek icin
function showInfoModal(options) {
    var modal = document.getElementById('sharedInfoModal');
    var bsModal = new bootstrap.Modal(modal);

    document.getElementById('infoModalTitle').textContent = options.title || 'Detaylar';
    document.getElementById('infoModalContent').innerHTML = options.content || '';

    bsModal.show();
}

// Onay butonu click handler
document.addEventListener('DOMContentLoaded', function () {
    var confirmBtn = document.getElementById('confirmModalBtn');
    if (confirmBtn) {
        confirmBtn.addEventListener('click', function () {
            var modal = bootstrap.Modal.getInstance(document.getElementById('sharedConfirmModal'));
            modal.hide();

            if (typeof confirmCallback === 'function') {
                confirmCallback();
            }
        });
    }
});
