document.addEventListener("DOMContentLoaded", function () {
    var menu = document.getElementById("navUserMenu");
    var btn = document.getElementById("navUserBtn");
    if (menu && btn) {
        btn.addEventListener("click", function (e) {
            e.stopPropagation();
            menu.classList.toggle("open");
        });
        document.addEventListener("click", function () {
            menu.classList.remove("open");
        });
    }

    document.querySelectorAll("[data-close]").forEach(function (el) {
        el.addEventListener("click", function () {
            var overlay = el.closest(".modal-overlay");
            if (overlay) overlay.classList.remove("open");
        });
    });

    document.querySelectorAll(".modal-overlay").forEach(function (overlay) {
        overlay.addEventListener("click", function (e) {
            if (e.target === overlay) overlay.classList.remove("open");
        });
    });

    var openModal = document.getElementById("openBorrowModal");
    if (openModal) {
        openModal.addEventListener("click", function () {
            var modal = document.getElementById("confirmBorrowModal");
            if (modal) modal.classList.add("open");
        });
    }
});
