/* quiz.js — loaded globally */
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

// Quiz navigation — only runs when the quiz form is present
(function () {
    const progressBar = document.getElementById('quiz-progress-bar');
    if (!progressBar) return;

    const totalQuestions = parseInt(progressBar.getAttribute('aria-valuemax'), 10);
    const progressFill = document.getElementById('progress-fill');
    const progressLabel = document.getElementById('progress-label');
    const announcement = document.getElementById('quiz-progress-announcement');

    function showQuestion(index) {
        document.querySelectorAll('.quiz-question').forEach(el => el.classList.add('d-none'));
        const target = document.getElementById('question-' + index);
        if (target) {
            target.classList.remove('d-none');
            const legend = target.querySelector('legend');
            if (legend) { legend.setAttribute('tabindex', '-1'); legend.focus(); }
        }
        updateProgress(index);
    }

    function updateProgress(index) {
        const pct = Math.round((index / totalQuestions) * 100);
        progressFill.style.width = pct + '%';
        progressBar.setAttribute('aria-valuenow', index);
        const label = 'Question ' + (index + 1) + ' of ' + totalQuestions;
        progressLabel.textContent = label;
        announcement.textContent = label;
    }

    function validateCurrent(index) {
        const radios = document.querySelectorAll('input[name="Selections[' + index + '].SelectedAnswerId"]');
        return Array.from(radios).some(r => r.checked);
    }

    document.querySelectorAll('.quiz-next-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const current = parseInt(this.dataset.current, 10);
            const target = parseInt(this.dataset.target, 10);
            if (!validateCurrent(current)) {
                const section = document.getElementById('question-' + current);
                let err = section.querySelector('.answer-error');
                if (!err) {
                    err = document.createElement('p');
                    err.className = 'text-danger mt-2 answer-error';
                    err.setAttribute('role', 'alert');
                    err.textContent = 'Please select an answer before continuing.';
                    section.querySelector('.list-group').after(err);
                }
                return;
            }
            const section = document.getElementById('question-' + current);
            const err = section.querySelector('.answer-error');
            if (err) err.remove();
            showQuestion(target);
        });
    });

    document.querySelectorAll('.quiz-prev-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            showQuestion(parseInt(this.dataset.target, 10));
        });
    });

    document.querySelector('.quiz-submit-btn')?.addEventListener('click', function (e) {
        const current = parseInt(this.dataset.current, 10);
        if (!validateCurrent(current)) {
            e.preventDefault();
            const section = document.getElementById('question-' + current);
            let err = section.querySelector('.answer-error');
            if (!err) {
                err = document.createElement('p');
                err.className = 'text-danger mt-2 answer-error';
                err.setAttribute('role', 'alert');
                err.textContent = 'Please select an answer before submitting.';
                section.querySelector('.list-group').after(err);
            }
        }
    });

    document.querySelectorAll('.quiz-answer-radio').forEach(radio => {
        radio.addEventListener('change', function () {
            const name = this.name;
            document.querySelectorAll('input[name="' + name + '"]').forEach(r => {
                r.closest('.quiz-answer-label').classList.remove('active', 'list-group-item-primary');
            });
            this.closest('.quiz-answer-label').classList.add('active', 'list-group-item-primary');
        });
    });

    updateProgress(0);
})();
