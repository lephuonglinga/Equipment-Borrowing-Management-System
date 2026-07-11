function showEbmsToast(message, type) {
    var host = document.getElementById("ebmsToastHost");
    if (!host || !message) {
        return;
    }

    var toast = document.createElement("div");
    toast.className = "ebms-toast ebms-toast-" + (type === "success" ? "success" : "error");
    toast.innerHTML =
        '<i class="fa-solid ' + (type === "success" ? "fa-circle-check" : "fa-circle-exclamation") + '"></i>' +
        "<span>" + message + "</span>";

    host.appendChild(toast);
    requestAnimationFrame(function () {
        toast.classList.add("show");
    });

    window.setTimeout(function () {
        toast.classList.remove("show");
        window.setTimeout(function () {
            toast.remove();
        }, 300);
    }, 4500);
}

window.showEbmsToast = showEbmsToast;

document.addEventListener("DOMContentLoaded", function () {
    if (window.__ebmsToast) {
        showEbmsToast(window.__ebmsToast.message, window.__ebmsToast.type);
    }

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

    var borrowForm = document.getElementById("borrowSubmitForm");
    if (borrowForm) {
        var borrowDateInput = document.getElementById("borrowDate");
        var returnDateInput = document.getElementById("expectedReturnDate");
        var purposeInput = document.getElementById("borrowPurpose");

        if (borrowDateInput && returnDateInput) {
            borrowDateInput.addEventListener("change", function () {
                returnDateInput.min = borrowDateInput.value || borrowDateInput.min;
                if (returnDateInput.value && returnDateInput.value < returnDateInput.min) {
                    returnDateInput.value = returnDateInput.min;
                }
            });
        }

        borrowForm.addEventListener("submit", function (e) {
            var borrowDate = borrowDateInput ? borrowDateInput.value : "";
            var returnDate = returnDateInput ? returnDateInput.value : "";
            var purpose = purposeInput ? purposeInput.value.trim() : "";
            var today = new Date().toISOString().slice(0, 10);
            var error = "";

            if (!borrowDate) {
                error = "Vui lòng chọn ngày mượn.";
            } else if (borrowDate < today) {
                error = "Ngày mượn không được ở trong quá khứ.";
            } else if (!returnDate) {
                error = "Vui lòng chọn ngày trả dự kiến.";
            } else if (returnDate < borrowDate) {
                error = "Ngày trả dự kiến phải sau hoặc bằng ngày mượn.";
            } else if (!purpose) {
                error = "Mục đích là bắt buộc.";
            }

            if (error) {
                e.preventDefault();
                showEbmsToast(error, "error");
            }
        });
    }

    var rejectModal = document.getElementById("rejectModal");
    if (rejectModal) {
        var rejectIdInput = document.getElementById("rejectRequestId");
        var rejectTabInput = document.getElementById("rejectTab");
        var rejectReasonInput = document.getElementById("rejectReason");

        document.querySelectorAll("[data-open-reject]").forEach(function (button) {
            button.addEventListener("click", function () {
                if (rejectIdInput) rejectIdInput.value = button.getAttribute("data-request-id") || "";
                if (rejectTabInput) rejectTabInput.value = button.getAttribute("data-request-tab") || "";
                if (rejectReasonInput) {
                    rejectReasonInput.value = "";
                    rejectReasonInput.focus();
                }
                rejectModal.classList.add("open");
            });
        });

        var rejectForm = document.getElementById("rejectForm");
        if (rejectForm) {
            rejectForm.addEventListener("submit", function (e) {
                var reason = rejectReasonInput ? rejectReasonInput.value.trim() : "";
                if (!reason) {
                    e.preventDefault();
                    showEbmsToast("Vui lòng nhập lý do từ chối.", "error");
                }
            });
        }
    }

    function requireText(value, label) {
        return value && value.trim() ? null : label + " là bắt buộc.";
    }

    function attachRequiredTextForm(formId, fields) {
        var form = document.getElementById(formId);
        if (!form) return;

        form.addEventListener("submit", function (e) {
            for (var i = 0; i < fields.length; i++) {
                var input = document.getElementById(fields[i].id);
                var value = input ? input.value : "";
                var error = requireText(value, fields[i].label);
                if (error) {
                    e.preventDefault();
                    showEbmsToast(error, "error");
                    if (input) input.focus();
                    return;
                }
            }
        });
    }

    attachRequiredTextForm("equipmentForm", [
        { id: "equipmentName", label: "Tên thiết bị" },
        { id: "equipmentSerial", label: "Số serial" },
        { id: "equipmentLocation", label: "Vị trí" }
    ]);

    attachRequiredTextForm("categoryForm", [
        { id: "categoryName", label: "Tên danh mục" }
    ]);

    attachRequiredTextForm("userCreateForm", [
        { id: "staffFullName", label: "Họ tên" },
        { id: "staffEmail", label: "Email" }
    ]);

    var maintenanceModal = document.getElementById("maintenanceModal");
    if (maintenanceModal) {
        document.querySelectorAll("[data-open-maintenance]").forEach(function (button) {
            button.addEventListener("click", function () {
                var idInput = document.getElementById("maintenanceEquipmentId");
                var nameEl = document.getElementById("maintenanceEquipmentName");
                var statusFilter = document.getElementById("maintenanceStatusFilter");
                var pageNumber = document.getElementById("maintenancePageNumber");
                if (idInput) idInput.value = button.getAttribute("data-equipment-id") || "";
                if (nameEl) nameEl.textContent = "Thiết bị: " + (button.getAttribute("data-equipment-name") || "");
                if (statusFilter) statusFilter.value = button.getAttribute("data-status-filter") || "";
                if (pageNumber) pageNumber.value = button.getAttribute("data-page-number") || "1";
                maintenanceModal.classList.add("open");
            });
        });
    }

    var returnForm = document.getElementById("returnForm");
    if (returnForm) {
        returnForm.addEventListener("submit", function (e) {
            var selects = returnForm.querySelectorAll(".return-status-select");
            for (var i = 0; i < selects.length; i++) {
                if (!selects[i].value) {
                    e.preventDefault();
                    showEbmsToast("Vui lòng chọn trạng thái cho từng thiết bị khi trả.", "error");
                    selects[i].focus();
                    return;
                }
            }
        });
    }

    var registerForm = document.getElementById("registerForm");
    if (registerForm) {
        registerForm.addEventListener("submit", function (e) {
            var fullName = document.getElementById("registerFullName");
            var email = document.getElementById("registerEmail");
            var error = requireText(fullName ? fullName.value : "", "Họ tên")
                || requireText(email ? email.value : "", "Email");
            if (error) {
                e.preventDefault();
                showEbmsToast(error, "error");
            }
        });
    }
});
