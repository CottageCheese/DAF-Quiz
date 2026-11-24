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
    const announcement = document.getElementById('quiz-progress-announcement');
    const dots = document.querySelectorAll('#quiz-dots .q-dot');

    let currentIndex = 0;

    function showQuestion(index) {
        document.querySelectorAll('.quiz-question').forEach(el => el.classList.add('d-none'));
        const target = document.getElementById('question-' + index);
        if (target) {
            target.classList.remove('d-none');
            const heading = target.querySelector('.q-text');
            if (heading) heading.focus();
        }
        currentIndex = index;
        updateProgress(index);
    }

    function updateProgress(index) {
        const pct = Math.round(((index + 1) / totalQuestions) * 100);
        if (progressFill) progressFill.style.width = pct + '%';
        progressBar.setAttribute('aria-valuenow', index + 1);
        const label = 'Question ' + (index + 1) + ' of ' + totalQuestions;
        if (announcement) announcement.textContent = label;
        dots.forEach((dot, i) => {
            dot.classList.toggle('done', i < index);
            dot.classList.toggle('current', i === index);
        });
    }

    function validateCurrent(index) {
        const radios = document.querySelectorAll('input[name="Selections[' + index + '].SelectedAnswerId"]');
        return Array.from(radios).some(r => r.checked);
    }

    function showError(index, message) {
        const section = document.getElementById('question-' + index);
        if (!section) return;
        let err = section.querySelector('.answer-error');
        if (!err) {
            err = document.createElement('p');
            err.className = 'answer-error';
            err.setAttribute('role', 'alert');
            section.querySelector('.answers')?.after(err);
        }
        err.textContent = message;
    }

    function clearError(index) {
        const section = document.getElementById('question-' + index);
        const err = section?.querySelector('.answer-error');
        if (err) err.remove();
    }

    function goNext() {
        if (!validateCurrent(currentIndex)) {
            showError(currentIndex, 'Please select an answer before continuing.');
            return;
        }
        clearError(currentIndex);
        if (currentIndex < totalQuestions - 1) {
            showQuestion(currentIndex + 1);
        }
    }

    document.querySelectorAll('.quiz-next-btn').forEach(btn => {
        btn.addEventListener('click', goNext);
    });

    document.querySelectorAll('.quiz-prev-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            showQuestion(parseInt(this.dataset.target, 10));
        });
    });

    document.querySelector('.quiz-submit-btn')?.addEventListener('click', function (e) {
        if (!validateCurrent(currentIndex)) {
            e.preventDefault();
            showError(currentIndex, 'Please select an answer before submitting.');
        }
    });

    // Reflect radio selection on .ans tile
    document.querySelectorAll('.quiz-answer-radio').forEach(radio => {
        radio.addEventListener('change', function () {
            const name = this.name;
            document.querySelectorAll('input[name="' + name + '"]').forEach(r => {
                r.closest('.ans')?.classList.remove('selected');
                r.closest('.ans')?.setAttribute('aria-checked', 'false');
            });
            const tile = this.closest('.ans');
            if (tile) {
                tile.classList.add('selected');
                tile.setAttribute('aria-checked', 'true');
            }
            clearError(currentIndex);
        });
    });

    // Keyboard shortcuts: A/B/C/D selects, Enter advances
    const letterToIndex = { a: 0, b: 1, c: 2, d: 3, e: 4, f: 5 };
    document.addEventListener('keydown', function (e) {
        if (e.ctrlKey || e.metaKey || e.altKey) return;
        const tag = (e.target.tagName || '').toUpperCase();
        if (tag === 'INPUT' && e.target.type !== 'radio') return;
        if (tag === 'TEXTAREA') return;

        const key = e.key.toLowerCase();
        if (key in letterToIndex) {
            const section = document.getElementById('question-' + currentIndex);
            const radios = section?.querySelectorAll('.quiz-answer-radio');
            const target = radios?.[letterToIndex[key]];
            if (target) {
                target.checked = true;
                target.dispatchEvent(new Event('change', { bubbles: true }));
                e.preventDefault();
            }
        } else if (e.key === 'Enter') {
            if (currentIndex < totalQuestions - 1) {
                e.preventDefault();
                goNext();
            }
        }
    });

    // Elapsed-time timer
    const timerValue = document.getElementById('quiz-timer-value');
    if (timerValue) {
        const started = Date.now();
        setInterval(() => {
            const s = Math.floor((Date.now() - started) / 1000);
            const mm = String(Math.floor(s / 60)).padStart(2, '0');
            const ss = String(s % 60).padStart(2, '0');
            timerValue.textContent = mm + ':' + ss;
        }, 1000);
    }

    updateProgress(0);
})();
