/**
 * Bridgo Landing Page — Waitlist
 */
(function () {
    'use strict';

    var form = document.getElementById('waitlistForm');
    var input = document.getElementById('waitlistEmail');
    var btn = document.getElementById('waitlistBtn');
    var btnText = document.getElementById('waitlistBtnText');
    var spinner = document.getElementById('waitlistSpinner');
    var errorEl = document.getElementById('waitlistError');
    var successEl = document.getElementById('waitlistSuccess');
    var formEl = document.getElementById('waitlistFormInner');
    var countEl = document.getElementById('waitlistCount');

    // Load waitlist count
    loadCount();

    // Form submit
    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            hideError();

            var email = input.value.trim();
            if (!email) {
                showError('Please enter your email address.');
                return;
            }
            if (!isValidEmail(email)) {
                showError('Please enter a valid email address.');
                return;
            }

            setLoading(true);

            $.ajax({
                url: '/api/waitlist',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ email: email, source: 'landing-page' }),
                success: function (data) {
                    setLoading(false);
                    if (data.success) {
                        showSuccess();
                        loadCount();
                    } else {
                        showError(data.message || 'Something went wrong. Please try again.');
                    }
                },
                error: function () {
                    setLoading(false);
                    showError('Connection error. Please try again.');
                }
            });
        });
    }

    // Enter key
    if (input) {
        input.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                form.dispatchEvent(new Event('submit'));
            }
        });
    }

    function loadCount() {
        $.get('/api/waitlist/count', function (data) {
            if (data.count > 0 && countEl) {
                countEl.textContent = data.count;
                var proofEl = document.getElementById('socialProof');
                if (proofEl) proofEl.style.display = 'flex';
            }
        });
    }

    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    function setLoading(loading) {
        btn.disabled = loading;
        btnText.style.display = loading ? 'none' : 'inline';
        spinner.style.display = loading ? 'inline-block' : 'none';
    }

    function showError(msg) {
        errorEl.textContent = msg;
        errorEl.style.display = 'block';
    }

    function hideError() {
        errorEl.style.display = 'none';
    }

    function showSuccess() {
        formEl.style.display = 'none';
        successEl.style.display = 'block';
    }
})();

/**
 * Copy wallet address to clipboard
 */
function copyAddress(address, btnEl) {
    if (navigator.clipboard) {
        navigator.clipboard.writeText(address).then(function () {
            showCopied(btnEl);
        });
    } else {
        // Fallback
        var ta = document.createElement('textarea');
        ta.value = address;
        ta.style.position = 'fixed';
        ta.style.opacity = '0';
        document.body.appendChild(ta);
        ta.select();
        document.execCommand('copy');
        document.body.removeChild(ta);
        showCopied(btnEl);
    }
}

function showCopied(btnEl) {
    var icon = btnEl.querySelector('i');
    icon.className = 'bi bi-check-lg';
    btnEl.classList.add('copied');
    setTimeout(function () {
        icon.className = 'bi bi-clipboard';
        btnEl.classList.remove('copied');
    }, 2000);
}
