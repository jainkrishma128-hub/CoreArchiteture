// Admin Products Index Page - JavaScript Module
// Handles CRUD operations, pagination, sorting, and search functionality

$(document).ready(function () {
    // State variables
    const areaPrefix = window.adminAreaPrefix || '/Admin';
    let currentProductId = null;
    let isEditMode = false;
    let currentPage = 1;
    let pageSize = 10;
    let sortBy = 'Id';
    let sortOrder = 'asc';
    let searchTerm = '';
    let categoryId = '';
    let searchTimeout = null;
    const token = $('input[name="__RequestVerificationToken"]').val();

    // Initialize
    loadProducts();
    loadCategories();
    setupCategoryFilter();
    initializeValidation();
    attachEventHandlers();

    // ========================================
    // Validation Setup
    // ========================================
    function initializeValidation() {
        $("#productForm").validate({
            rules: {
                Name: { required: true, maxlength: 100 },
                Description: { required: true, maxlength: 500 },
                CategoryId: { required: true },
                Price: { required: true, number: true, min: 0.01 }
            },
            errorPlacement: function (error, element) {
                $(`#error-${element.attr('name')}`).html(error);
            },
            success: function (label, element) {
                $(`#error-${$(element).attr('name')}`).html('');
            }
        });
    }

    // ========================================
    // Event Handlers
    // ========================================
    function attachEventHandlers() {
        // Search functionality with debounce
        $('#searchBox').on('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                searchTerm = $('#searchBox').val();
                currentPage = 1;
                loadProducts();
            }, 300);
        });

        // Clear search
        $('#btnClearSearch').click(function () {
            $('#searchBox').val('');
            $('#categoryFilter').val('');
            searchTerm = '';
            categoryId = '';
            currentPage = 1;
            loadProducts();
        });

        // Page size change
        $('#pageSizeSelect').change(function () {
            pageSize = parseInt($(this).val());
            currentPage = 1;
            loadProducts();
        });

        // Column sorting
        $(document).on('click', '.sortable', function () {
            const column = $(this).data('column');
            if (sortBy === column) {
                sortOrder = sortOrder === 'asc' ? 'desc' : 'asc';
            } else {
                sortBy = column;
                sortOrder = 'asc';
            }
            updateSortIndicators();
            loadProducts();
        });

        // Pagination clicks
        $(document).on('click', '#pagination a', function (e) {
            e.preventDefault();
            const page = parseInt($(this).data('page'));
            if (page && page !== currentPage) {
                currentPage = page;
                loadProducts();
            }
        });

        // Create product button
        $('#btnCreateProduct').click(function () {
            isEditMode = false;
            currentProductId = null;
            $('#productModalLabel').text('Create Product');
            $('#saveButtonText').text('Save Product');
            resetForm();
            $('#productModal').modal('show');
        });

        // Import form submission
        $('#importForm').on('submit', function (e) {
            e.preventDefault();
            const fileInput = $('#importFile')[0];
            if (fileInput.files.length === 0) {
                showAlert('warning', 'Please select a file');
                return;
            }

            const formData = new FormData();
            formData.append('file', fileInput.files[0]);

            $('#btnImport').prop('disabled', true);
            $('#importSpinner').removeClass('d-none');
            $('#importButtonText').text('Importing...');

            $.ajax({
                url: `${areaPrefix}/Products/Import`,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.success) {
                        $('#importModal').modal('hide');
                        $('#importForm')[0].reset();
                        showAlert('success', 'Products imported successfully');
                        loadProducts();
                    } else {
                        showAlert('danger', response.message || 'Import failed');
                    }
                },
                error: function () {
                    showAlert('danger', 'An error occurred during import');
                },
                complete: function () {
                    $('#btnImport').prop('disabled', false);
                    $('#importSpinner').addClass('d-none');
                    $('#importButtonText').text('Import Products');
                }
            });
        });

        // Form submission for Create/Edit
        $('#productForm').submit(function (e) {
            e.preventDefault();
            if (!$(this).valid()) return;
            saveProduct();
        });

        // Confirm delete
        $('#btnConfirmDelete').click(function () {
            deleteProduct();
        });

        // Reset form on modal close
        $('#productModal').on('hidden.bs.modal', resetForm);
    }

    // ========================================
    // Sorting Functions
    // ========================================
    function updateSortIndicators() {
        $('.sortable').removeClass('active');
        $('.sortable i').removeClass('fas fa-sort-up fas fa-sort-down').addClass('fas fa-sort');
        const activeHeader = $(`.sortable[data-column="${sortBy}"]`);
        activeHeader.addClass('active');
        activeHeader.find('i').removeClass('fas fa-sort')
            .addClass(sortOrder === 'asc' ? 'fas fa-sort-up' : 'fas fa-sort-down');
    }

    // ========================================
    // Data Loading Functions
    // ========================================
    function loadProducts() {
        $('#loadingIndicator').show();
        $('#productsTable').hide();
        $('#emptyState').hide();

        const params = {
            PageNumber: currentPage,
            PageSize: pageSize,
            SortBy: sortBy,
            SortOrder: sortOrder
        };

        if (searchTerm) params.SearchTerm = searchTerm;
        if (categoryId) params.CategoryId = categoryId;

        $.ajax({
            url: `${areaPrefix}/Products/GetAll`,
            type: 'GET',
            data: params,
            success: function (response) {
                $('#loadingIndicator').hide();
                if (response.success) {
                    renderProducts(response.data);
                }
            },
            error: function (xhr) {
                $('#loadingIndicator').hide();
                console.error('API Error:', xhr);
                showAlert('danger', 'Error loading products. Check console for details.');
            }
        });
    }

    function loadCategories(callback) {
        $.ajax({
            url: `${areaPrefix}/Products/GetCategories`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const categorySelect = $('#CategoryId');
                    categorySelect.empty();
                    categorySelect.append('<option value="">Select Category</option>');

                    response.data.forEach(function (item) {
                        const id = item.id || item.Id;
                        const name = item.name || item.Name;
                        categorySelect.append(`<option value="${id}">${name}</option>`);
                    });

                    if (typeof callback === 'function') callback();
                }
            }
        });
    }

    function setupCategoryFilter() {
        $.ajax({
            url: `${areaPrefix}/Products/GetCategories`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const filter = $('#categoryFilter');
                    response.data.forEach(cat => {
                        const id = cat.id || cat.Id;
                        const name = cat.name || cat.Name;
                        filter.append(`<option value="${id}">${name}</option>`);
                    });
                }
            }
        });

        $('#categoryFilter').on('change', function () {
            categoryId = $(this).val();
            currentPage = 1;
            loadProducts();
        });
    }

    // ========================================
    // Rendering Functions
    // ========================================
    function renderProducts(result) {
        const tbody = $('#productsTableBody');
        tbody.empty();

        if (result.items.length === 0) {
            $('#productsTable').hide();
            $('#emptyState').show();
            $('#pagination').empty();
            $('#resultInfo').text('');
            return;
        }

        $('#productsTable').show();
        $('#emptyState').hide();

        result.items.forEach(function (product) {
            const row = `
                <tr>
                    <td class="ps-4 text-muted small">${product.id}</td>
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="product-icon me-3 shadow-sm border d-none d-sm-flex">
                                <i class="fas fa-box text-primary"></i>
                            </div>
                            <div>
                                <h6 class="mb-0 fw-bold text-dark">${escapeHtml(product.name)}</h6>
                                <small class="text-muted d-none d-md-block text-truncate" style="max-width: 250px;">${escapeHtml(product.description)}</small>
                            </div>
                        </div>
                    </td>
                    <td class="d-none d-md-table-cell">
                        <span class="badge bg-light text-primary border fw-medium">${escapeHtml(product.categoryName || 'Uncategorized')}</span>
                    </td>
                    <td class="d-none d-lg-table-cell text-end">
                        <span class="fw-bold text-dark">$${product.price.toFixed(2)}</span>
                    </td>
                    <td class="pe-4 text-end">
                        <div class="d-flex justify-content-end gap-1">
                            <button class="btn btn-light btn-sm rounded-pill px-2 d-md-none text-info fw-bold" onclick="viewProductDetail(${product.id})">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-light btn-sm rounded-pill px-3 text-primary fw-bold" onclick="editProduct(${product.id})">
                                <i class="fas fa-pencil-alt"></i><span class="d-none d-lg-inline ms-1">Edit</span>
                            </button>
                            <button class="btn btn-light btn-sm rounded-pill px-3 text-danger fw-bold" onclick="deleteProduct(${product.id}, '${escapeHtml(product.name)}')">
                                <i class="fas fa-trash-alt"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

        renderPagination(result);
        updateResultInfo(result);

        // Update stats
        $('#statTotalProducts').text(result.totalCount);
    }

    function renderPagination(result) {
        const pagination = $('#pagination');
        pagination.empty();

        if (result.totalPages <= 1) return;

        const prevDisabled = !result.hasPrevious ? 'disabled' : '';
        pagination.append(`
            <li class="page-item ${prevDisabled}">
                <a class="page-link rounded-start-pill" href="#" data-page="${result.pageNumber - 1}">
                    <i class="fas fa-chevron-left"></i>
                </a>
            </li>
        `);

        // Simple pagination logic
        for (let i = 1; i <= result.totalPages; i++) {
            if (i === 1 || i === result.totalPages || (i >= result.pageNumber - 2 && i <= result.pageNumber + 2)) {
                const active = i === result.pageNumber ? 'active' : '';
                pagination.append(`
                    <li class="page-item ${active}">
                        <a class="page-link" href="#" data-page="${i}">${i}</a>
                    </li>
                `);
            } else if (i === result.pageNumber - 3 || i === result.pageNumber + 3) {
                pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }
        }

        const nextDisabled = !result.hasNext ? 'disabled' : '';
        pagination.append(`
            <li class="page-item ${nextDisabled}">
                <a class="page-link rounded-end-pill" href="#" data-page="${result.pageNumber + 1}">
                    <i class="fas fa-chevron-right"></i>
                </a>
            </li>
        `);
    }

    function updateResultInfo(result) {
        const start = (result.pageNumber - 1) * result.pageSize + 1;
        const end = Math.min(result.pageNumber * result.pageSize, result.totalCount);
        $('#resultInfo').text(`Showing ${start}-${end} of ${result.totalCount} products`);
    }

    // ========================================
    // Global Access Functions (for onclick)
    // ========================================
    window.editProduct = function (productId) {
        isEditMode = true;
        currentProductId = productId;
        $('#productModalLabel').text('Edit Product');
        $('#saveButtonText').text('Update Product');

        $.ajax({
            url: `${areaPrefix}/Products/GetById/${productId}`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const product = response.data;
                    $('#productId').val(product.id);
                    $('#productName').val(product.name);
                    $('#productDescription').val(product.description);
                    $('#productPrice').val(product.price);
                    $('#CategoryId').val(product.categoryId || product.CategoryId);
                    $('#productModal').modal('show');
                }
            }
        });
    };

    window.deleteProduct = function (productId, name) {
        currentProductId = productId;
        $('#deleteProductName').text(name);
        $('#deleteModal').modal('show');
    };

    window.viewProductDetail = function (productId) {
        $.ajax({
            url: `${areaPrefix}/Products/GetById/${productId}`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const p = response.data;
                    $('#detailName').text(p.name);
                    $('#detailCategory').text(p.categoryName || 'Uncategorized');
                    $('#detailPrice').text('$' + p.price.toFixed(2));
                    $('#detailId').text(p.id);
                    $('#detailDescription').text(p.description);
                    $('#detailModal').modal('show');
                }
            }
        });
    };

    // ========================================
    // CRUD Operations
    // ========================================
    function saveProduct() {
        const formData = {
            Name: $('#productName').val(),
            Description: $('#productDescription').val(),
            CategoryId: parseInt($('#CategoryId').val()),
            Price: parseFloat($('#productPrice').val())
        };

        const url = isEditMode
            ? `${areaPrefix}/Products/Edit/${currentProductId}`
            : `${areaPrefix}/Products/Create`;

        $('#saveSpinner').removeClass('d-none');
        $('#btnSaveProduct').prop('disabled', true);

        $.ajax({
            url: url,
            type: isEditMode ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#productModal').modal('hide');
                    showAlert('success', response.message);
                    loadProducts();
                } else if (response.errors) {
                    for (const [field, messages] of Object.entries(response.errors)) {
                        $(`#error-${field}`).html(messages.join('<br>'));
                    }
                } else {
                    showAlert('danger', response.message || 'An error occurred.');
                }
            },
            error: function (xhr) {
                showAlert('danger', 'Error saving product. Check console.');
            },
            complete: function () {
                $('#saveSpinner').addClass('d-none');
                $('#btnSaveProduct').prop('disabled', false);
            }
        });
    }

    function deleteProduct() {
        $('#deleteSpinner').removeClass('d-none');
        $('#btnConfirmDelete').prop('disabled', true);

        $.ajax({
            url: `${areaPrefix}/Products/Delete/${currentProductId}`,
            type: 'DELETE',
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#deleteModal').modal('hide');
                    showAlert('success', response.message);
                    loadProducts();
                }
            },
            complete: function () {
                $('#deleteSpinner').addClass('d-none');
                $('#btnConfirmDelete').prop('disabled', false);
            }
        });
    }

    // ========================================
    // Utility Functions
    // ========================================
    function resetForm() {
        $('#productForm')[0].reset();
        $('#productForm').validate().resetForm();
        $('.text-danger').html('');
    }

    function showAlert(type, message) {
        const alert = `
            <div class="alert alert-${type} alert-dismissible fade show border-0 shadow-sm" role="alert">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        $('#alertContainer').html(alert);
        setTimeout(() => { $('.alert').fadeOut(() => $(this).remove()); }, 4000);
    }

    function escapeHtml(text) {
        if (!text) return '';
        const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
        return text.replace(/[&<>"']/g, m => map[m]);
    }
});
