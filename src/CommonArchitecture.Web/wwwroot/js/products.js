// Products Index Page - JavaScript Module
// Handles CRUD operations, pagination, sorting, and search functionality

$(document).ready(function () {
    // State variables
    let currentProductId = null;
    let isEditMode = false;
    let currentPage = 1;
    let pageSize = 10;
    let sortBy = 'Id';
    let sortOrder = 'asc';
    let searchTerm = '';
    let searchTimeout = null;
    const token = $('input[name="__RequestVerificationToken"]').val();

    // Initialize
    loadProducts();
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
                Price: { required: true, number: true, min: 0.01 },
                Stock: { required: true, digits: true, min: 0 }
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
            searchTerm = '';
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

        // Edit product button
        $(document).on('click', '.btn-edit', function () {
            const productId = $(this).data('id');
            loadProductForEdit(productId);
        });

        // Delete product button
        $(document).on('click', '.btn-delete', function () {
            currentProductId = $(this).data('id');
            $('#deleteProductName').text($(this).data('name'));
            $('#deleteModal').modal('show');
        });

        // Form submission
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
        $('.sortable i').removeClass('bi-arrow-up bi-arrow-down').addClass('bi-arrow-down-up');
        const activeHeader = $(`.sortable[data-column="${sortBy}"]`);
        activeHeader.addClass('active');
        activeHeader.find('i').removeClass('bi-arrow-down-up')
            .addClass(sortOrder === 'asc' ? 'bi-arrow-up' : 'bi-arrow-down');
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

        $.ajax({
            url: '/Products/GetAll',
            type: 'GET',
            data: params,
            success: function (response) {
                $('#loadingIndicator').hide();
                if (response.success) {
                    renderProducts(response.data);
                }
            },
            error: function () {
                $('#loadingIndicator').hide();
                showAlert('danger', 'Error loading products. Please try again.');
            }
        });
    }

    function loadProductForEdit(productId) {
        isEditMode = true;
        currentProductId = productId;
        $('#productModalLabel').text('Edit Product');
        $('#saveButtonText').text('Update Product');

        $.ajax({
            url: `/Products/GetById/${productId}`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const product = response.data;
                    $('#productId').val(product.id);
                    $('#productName').val(product.name);
                    $('#productDescription').val(product.description);
                    $('#productPrice').val(product.price);
                    $('#productStock').val(product.stock);
                    $('#productModal').modal('show');
                } else {
                    showAlert('danger', 'Failed to load product details.');
                }
            },
            error: function () {
                showAlert('danger', 'Error loading product details. Please try again.');
            }
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
            const stockBadge = product.stock > 0
                ? `<span class="badge bg-success">${product.stock}</span>`
                : `<span class="badge bg-danger">Out of Stock</span>`;

            const row = `
                <tr>
                    <td>${product.id}</td>
                    <td>${escapeHtml(product.name)}</td>
                    <td>${escapeHtml(product.description)}</td>
                    <td>$${product.price.toFixed(2)}</td>
                    <td>${stockBadge}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-warning btn-edit" data-id="${product.id}">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-danger btn-delete" data-id="${product.id}" data-name="${escapeHtml(product.name)}">
                            <i class="bi bi-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

        renderPagination(result);
        updateResultInfo(result);
    }

    function renderPagination(result) {
        const pagination = $('#pagination');
        pagination.empty();

        if (result.totalPages <= 1) return;

        const prevDisabled = !result.hasPrevious ? 'disabled' : '';
        pagination.append(`
            <li class="page-item ${prevDisabled}">
                <a class="page-link" href="#" data-page="${result.pageNumber - 1}">Previous</a>
            </li>
        `);

        const startPage = Math.max(1, result.pageNumber - 2);
        const endPage = Math.min(result.totalPages, result.pageNumber + 2);

        for (let i = startPage; i <= endPage; i++) {
            const active = i === result.pageNumber ? 'active' : '';
            pagination.append(`
                <li class="page-item ${active}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `);
        }

        const nextDisabled = !result.hasNext ? 'disabled' : '';
        pagination.append(`
            <li class="page-item ${nextDisabled}">
                <a class="page-link" href="#" data-page="${result.pageNumber + 1}">Next</a>
            </li>
        `);
    }

    function updateResultInfo(result) {
        const start = (result.pageNumber - 1) * result.pageSize + 1;
        const end = Math.min(result.pageNumber * result.pageSize, result.totalCount);
        $('#resultInfo').text(`Showing ${start}-${end} of ${result.totalCount} products`);
    }

    // ========================================
    // CRUD Operations
    // ========================================
    function saveProduct() {
        const formData = {
            Name: $('#productName').val(),
            Description: $('#productDescription').val(),
            Price: parseFloat($('#productPrice').val()),
            Stock: parseInt($('#productStock').val())
        };

        const url = isEditMode
            ? `/Products/Edit/${currentProductId}`
            : '/Products/Create';

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
                    // Display server-side validation errors
                    for (const [field, messages] of Object.entries(response.errors)) {
                        $(`#error-${field}`).html(messages.join('<br>'));
                    }
                } else {
                    showAlert('danger', response.message || 'An error occurred.');
                }
            },
            error: function () {
                showAlert('danger', 'An error occurred while saving the product. Please try again.');
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
            url: `/Products/Delete/${currentProductId}`,
            type: 'DELETE',
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#deleteModal').modal('hide');
                    showAlert('success', response.message);
                    loadProducts();
                } else {
                    showAlert('danger', response.message || 'Failed to delete product.');
                }
            },
            error: function () {
                showAlert('danger', 'An error occurred while deleting the product. Please try again.');
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
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        $('#alertContainer').html(alert);
        setTimeout(function () {
            $('.alert').fadeOut('slow', function () { $(this).remove(); });
        }, 5000);
    }

    function escapeHtml(text) {
        const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
        return text.replace(/[&<>"']/g, function (m) { return map[m]; });
    }
});
