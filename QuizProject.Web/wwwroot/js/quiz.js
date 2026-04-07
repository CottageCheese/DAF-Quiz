/* quiz.js — loaded globally; quiz page logic is inline in Take.cshtml */
'use strict';

// Auto-dismiss Bootstrap alerts after 8 seconds
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
            setTimeout(function () {
                var bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
                if (bsAlert) bsAlert.close();
            }, 8000);
        });
    });
})();
