document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("navbarSearchInput");
    const searchButton = document.getElementById("navbarSearchButton");

    // Tạo container hiển thị kết quả tìm kiếm (dropdown)
    let searchResults = document.createElement("div");
    searchResults.id = "navbarSearchResults";
    searchResults.style.position = "absolute";
    searchResults.style.background = "#fff";
    searchResults.style.border = "1px solid #ccc";
    searchResults.style.boxShadow = "0 2px 4px rgba(0,0,0,0.2)";
    searchResults.style.zIndex = "1000";
    searchResults.style.display = "none";
    document.body.appendChild(searchResults);

    // Ẩn dropdown khi click ra ngoài
    document.addEventListener("click", function(e) {
        if (!searchResults.contains(e.target) && e.target !== searchInput && e.target !== searchButton) {
            searchResults.style.display = "none";
        }
    });

    // Cho phép bấm Enter để tìm kiếm
    searchInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") {
            performSearch();
        }
    });

    searchButton.addEventListener("click", performSearch);

    async function performSearch() {
        const query = searchInput.value.trim();
        if (!query) return;

        // Hiển thị thông báo tìm kiếm
        positionSearchResults();
        searchResults.innerHTML = "<div style='padding:10px;'>🔍 Đang tìm kiếm...</div>";
        searchResults.style.display = "block";

        try {
            // Gọi API theo route: /api/Search/{songName}
            const res = await fetch(`/api/Search/${encodeURIComponent(query)}`);
            const data = await res.json();
            console.log("API Response:", data);

            if (data.found) {
                // Hiển thị tên bài hát dưới dạng liên kết (chỉ hiện tên bài hát)
                searchResults.innerHTML = `<a href="${data.playLink}" style="display:block; padding:10px; text-decoration:none; color:#065fd4;">${data.title || data.Title}</a>`;
            } else {
                // Hiển thị cảnh báo trước khi chuyển sang trang liên hệ
                alert(`🚫 Hệ thống chưa có bài hát "${query}". Vui lòng liên hệ Admin.`);

                window.location.href = `/Contact?messageContent=${encodeURIComponent("Thêm bài \"" + query + "\" vào hệ thống.")}`;
            }
        } catch (error) {
            console.error("Error searching song:", error);
            searchResults.innerHTML = "<div style='padding:10px;'>❌ Lỗi khi tìm kiếm. Vui lòng thử lại!</div>";
        }
    }

    // Định vị và căn chỉnh dropdown kết quả ngay bên dưới thanh tìm kiếm trên navbar
    function positionSearchResults() {
        const rect = searchInput.getBoundingClientRect();
        searchResults.style.width = rect.width + "px";
        searchResults.style.left = rect.left + "px";
        searchResults.style.top = (rect.bottom + window.scrollY) + "px";
    }
});