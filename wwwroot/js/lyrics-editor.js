(function () {
    'use strict';

    var audio = document.getElementById('editorAudio');
    var playToggle = document.getElementById('playToggle');
    var playIcon = playToggle.querySelector('i');
    var currentTimeSpan = document.getElementById('currentTime');
    var durationSpan = document.getElementById('duration');
    var seekBar = document.getElementById('seekBar');
    var lyricsText = document.getElementById('lyricsText');
    var previewList = document.getElementById('previewList');
    var stampButton = document.getElementById('stampButton');
    var clearTimes = document.getElementById('clearTimes');
    var applyOffset = document.getElementById('applyOffset');
    var resetOffset = document.getElementById('resetOffset');
    var offsetInput = document.getElementById('offsetInput');
    var linesJsonInput = document.getElementById('linesJson');

    // Mỗi dòng: { time: số giây hoặc null, text: nội dung }
    var lines = [];
    var selectedIndex = 0;

    // --- Parser LRC (cứng cáp: bỏ qua tag meta, hỗ trợ nhiều timestamp/dòng, giờ:phút:giây) ---
    function parseLRC(text) {
        var result = [];
        (text || '').split(/\r?\n/).forEach(function (line) {
            line = line.trim();
            if (!line) return;
            if (/^\[(ti|ar|al|by|re|ve|length|offset):/i.test(line)) return;

            var timePattern = /\[(?:(\d{1,2}):)?(\d{1,2}):(\d{2}(?:\.\d{1,3})?)\]/g;
            var match;
            var timestamps = [];
            while ((match = timePattern.exec(line)) !== null) {
                var hours = match[1] ? parseInt(match[1], 10) : 0;
                var minutes = parseInt(match[2], 10);
                var seconds = parseFloat(match[3]);
                timestamps.push(hours * 3600 + minutes * 60 + seconds);
            }
            if (timestamps.length === 0) return;

            var content = line.replace(timePattern, '').trim();
            if (!content) return;

            timestamps.forEach(function (t) {
                result.push({ time: t, text: content });
            });
        });
        result.sort(function (a, b) { return a.time - b.time; });
        return result;
    }

    function formatTime(seconds) {
        if (seconds === null || seconds === undefined || isNaN(seconds) || seconds < 0) return '--:--';
        var m = Math.floor(seconds / 60);
        var s = Math.floor(seconds % 60);
        var c = Math.floor((seconds - Math.floor(seconds)) * 100);
        return m + ':' + (s < 10 ? '0' : '') + s + '.' + (c < 10 ? '0' : '') + c;
    }

    // Đồng bộ lines từ nội dung textarea, giữ thời gian đã gắn theo chỉ mục.
    function rebuildLinesFromText() {
        var raw = lyricsText.value.split('\n');
        while (raw.length > 0 && raw[raw.length - 1].trim() === '') raw.pop();

        var next = raw.map(function (text) {
            return { time: null, text: text };
        });
        lines.forEach(function (line, i) {
            if (i < next.length) next[i].time = line.time;
        });
        lines = next;
    }

    var currentActiveIndex = -1;

    function computeActiveIndex() {
        if (isNaN(audio.currentTime)) return -1;
        var sorted = lines.map(function (line, i) { return { index: i, time: line.time }; })
            .filter(function (item) { return item.time !== null; })
            .sort(function (a, b) { return a.time - b.time; });
        for (var i = 0; i < sorted.length; i++) {
            var nextTime = i < sorted.length - 1 ? sorted[i + 1].time : Infinity;
            if (audio.currentTime >= sorted[i].time && audio.currentTime < nextTime) {
                return sorted[i].index;
            }
        }
        return -1;
    }

    function updateActive() {
        var idx = computeActiveIndex();
        if (idx === currentActiveIndex) return;
        if (currentActiveIndex >= 0 && previewList.children[currentActiveIndex]) {
            previewList.children[currentActiveIndex].classList.remove('active');
        }
        currentActiveIndex = idx;
        if (idx >= 0 && previewList.children[idx]) {
            previewList.children[idx].classList.add('active');
        }
    }

    function renderPreview() {
        previewList.innerHTML = '';
        lines.forEach(function (line, i) {
            var li = document.createElement('li');
            li.className = 'editor-line' + (i === selectedIndex ? ' selected' : '');
            li.setAttribute('data-index', i);

            var timeSpan = document.createElement('span');
            timeSpan.className = 'editor-line-time';
            timeSpan.textContent = formatTime(line.time);

            var textSpan = document.createElement('span');
            textSpan.className = 'editor-line-text';
            textSpan.textContent = line.text;

            li.appendChild(timeSpan);
            li.appendChild(textSpan);
            previewList.appendChild(li);
        });
        currentActiveIndex = -1;
        updateActive();
    }

    function renderAll() {
        rebuildLinesFromText();
        if (selectedIndex >= lines.length) selectedIndex = Math.max(0, lines.length - 1);
        renderPreview();
        syncLinesJson();
    }

    function syncLinesJson() {
        linesJsonInput.value = JSON.stringify(lines.map(function (line) {
            return { time: line.time === null ? -1 : line.time, text: line.text };
        }));
    }

    function stampSelected() {
        if (lines.length === 0) return;
        if (selectedIndex >= lines.length) selectedIndex = lines.length - 1;
        if (isNaN(audio.currentTime) || audio.duration === undefined) return;
        lines[selectedIndex].time = audio.currentTime;
        if (selectedIndex < lines.length - 1) {
            selectedIndex++;
        }
        renderPreview();
        syncLinesJson();
    }

    // --- Sự kiện ---
    lyricsText.addEventListener('input', renderAll);

    playToggle.addEventListener('click', function () {
        if (audio.paused) {
            audio.play();
        } else {
            audio.pause();
        }
    });

    audio.addEventListener('play', function () {
        playIcon.className = 'fa fa-pause';
    });
    audio.addEventListener('pause', function () {
        playIcon.className = 'fa fa-play';
    });

    audio.addEventListener('loadedmetadata', function () {
        durationSpan.textContent = formatTime(audio.duration);
    });

    audio.addEventListener('timeupdate', function () {
        currentTimeSpan.textContent = formatTime(audio.currentTime);
        var progress = audio.duration ? (audio.currentTime / audio.duration) * 100 : 0;
        seekBar.value = progress;
        updateActive();
    });

    seekBar.addEventListener('input', function () {
        if (audio.duration) {
            audio.currentTime = (seekBar.value / 100) * audio.duration;
        }
    });

    stampButton.addEventListener('click', stampSelected);
    clearTimes.addEventListener('click', function () {
        lines.forEach(function (line) { line.time = null; });
        renderPreview();
        syncLinesJson();
    });

    document.getElementById('saveForm').addEventListener('submit', function (e) {
        syncLinesJson();
        var unstamped = lines.filter(function (line) { return line.time === null && line.text.trim() !== ''; }).length;
        if (unstamped > 0) {
            var proceed = window.confirm(
                'Có ' + unstamped + ' dòng chưa gắn thời gian và sẽ bị bỏ đi khi lưu. Tiếp tục lưu?'
            );
            if (!proceed) {
                e.preventDefault();
                return;
            }
        }
    });

    applyOffset.addEventListener('click', function () {
        var offset = parseFloat(offsetInput.value);
        if (isNaN(offset)) offset = 0;
        lines.forEach(function (line) {
            if (line.time !== null) line.time = Math.max(0, line.time + offset);
        });
        renderPreview();
        syncLinesJson();
    });

    resetOffset.addEventListener('click', function () {
        offsetInput.value = 0;
    });

    previewList.addEventListener('click', function (e) {
        var li = e.target.closest('li.editor-line');
        if (!li) return;
        selectedIndex = parseInt(li.getAttribute('data-index'), 10);
        renderPreview();
    });

    previewList.addEventListener('dblclick', function (e) {
        var li = e.target.closest('li.editor-line');
        if (!li) return;
        var index = parseInt(li.getAttribute('data-index'), 10);
        if (lines[index] && lines[index].time !== null) {
            audio.currentTime = lines[index].time;
            if (audio.paused) audio.play();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.target === lyricsText) {
            // Khi đang gõ lời: dùng Ctrl+T để gắn thời gian (T thường nằm trong lời bài hát).
            if (e.key === 't' && e.ctrlKey) {
                e.preventDefault();
                stampSelected();
            }
            return;
        }
        if (e.key === ' ' && e.target === document.body) {
            e.preventDefault();
            playToggle.click();
        }
        if (e.key === 't' || e.key === 'T') {
            stampSelected();
        }
    });

    // --- Khởi tạo ---
    var parsed = parseLRC(existingLrc);
    lines = parsed.map(function (p) {
        return { time: p.time, text: p.text };
    });
    lyricsText.value = lines.map(function (line) { return line.text; }).join('\n');
    renderAll();
})();