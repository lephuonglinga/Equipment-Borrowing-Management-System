function renderCategories(categories) {
  if (!categories || categories.length === 0) {
    $("#categoryGrid").html('<p class="empty-state">Chưa có danh mục.</p>');
    return;
  }

  let html = "";
  categories.forEach(function (cat) {
    html +=
      '<a class="category-card" href="equipment.html?categoryId=' +
      cat.id +
      '">' +
      '<i class="fa-solid fa-folder"></i>' +
      "<h3>" +
      escapeHtml(cat.name) +
      "</h3>" +
      "<p>" +
      escapeHtml(cat.description || "") +
      "</p>" +
      "</a>";
  });

  $("#categoryGrid").html(html);
}

$(document).ready(function () {
  if (!requireAuth()) return;
  renderAuthNav("categories");

  apiRequest({ url: "/api/equipment-categories" })
    .done(renderCategories)
    .fail(function (xhr) {
      $("#categoryGrid").html('<p class="empty-state">' + escapeHtml(getErrorMessage(xhr)) + "</p>");
    });
});
