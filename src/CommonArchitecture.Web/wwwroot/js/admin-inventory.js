$(document).ready(function () {
    let currentPage = 1;
    let pageSize = 10;
    let searchTerm = '';
    let categoryId = '';
    let sortBy = 'ProductName';
    let sortOrder = 'asc';

    // Initialize
    loadInventory();
    loadCategories();

    // Event Handlers
    $('#btnFilterInventory').click(function () {
        searchTerm = $('#inventorySearchBox').val();
        categoryId = $('#inventoryCategorySelect').val();
        currentPage = 1;
        loadInventory();
    });

    $('#inventorySearchBox').on('keypress', function (e) {
        if (e.which == 13) {
            $('#btnFilterInventory').click();
        }
    });

    $('#btnRefresh').click(function () {
        loadInventory();
    });

    $(document).on('click', '.page-link', function (e) {
        e.preventDefault();
        const page = $(this).data('page');
        if (page && page !== currentPage) {
            currentPage = page;
            loadInventory();
        }
    });

    $('#adjustmentForm').submit(function (e) {
        e.preventDefault();
        saveAdjustment();
    });

    // Functions
    function loadInventory() {
        $('#loadingInventory').show();
        $('#inventoryTable').hide();
        $('#emptyInventory').hide();

        const params = {
            PageNumber: currentPage,
            PageSize: pageSize,
            SearchTerm: searchTerm,
            CategoryId: categoryId || null,
            SortBy: sortBy,
            SortOrder: sortOrder
        };

        $.ajax({
            url: '/Admin/Inventory/GetAll',
            type: 'GET',
            data: params,
            success: function (response) {
                $('#loadingInventory').hide();
                if (response.success) {
                    renderInventory(response.data);
                } else {
                    alert(response.message || 'Error loading inventory');
                }
            },
            error: function () {
                $('#loadingInventory').hide();
                alert('An error occurred while loading inventory data');
            }
        });
    }

    function renderInventory(result) {
        const tbody = $('#inventoryTableBody');
        tbody.empty();

        if (result.items.length === 0) {
            $('#emptyInventory').show();
            return;
        }

        $('#inventoryTable').show();

        let totalStock = 0;
        let lowStockCount = 0;
        let outOfStockCount = 0;

        result.items.forEach(function (item) {
            totalStock += item.currentStock;
            if (item.currentStock <= 0) outOfStockCount++;
            else if (item.currentStock < 10) lowStockCount++;

            let stockBadgeClass = 'bg-success';
            if (item.currentStock <= 0) stockBadgeClass = 'bg-danger';
            else if (item.currentStock < 10) stockBadgeClass = 'bg-warning text-dark';

            const lastUpdated = item.lastUpdated ? new Date(item.lastUpdated).toLocaleString() : 'Never';

            const row = `
                <tr>
                    <td class="ps-4">
                        <div class="fw-bold text-dark">${item.productName}</div>
                        <small class="text-muted">ID: ${item.productId}</small>
                    </td>
                    <td><span class="badge bg-light text-dark border">${item.categoryName}</span></td>
                    <td>
                        <span class="badge ${stockBadgeClass} rounded-pill px-3 fs-6">${item.currentStock}</span>
                    </td>
                    <td class="text-muted small">${lastUpdated}</td>
                    <td class="pe-4 text-end">
                        <button class="btn btn-sm btn-outline-primary rounded-pill px-3 me-2" onclick="openAdjustmentModal(${item.productId}, '${item.productName.replace(/'/g, "\\'")}')">
                            <i class="fas fa-edit me-1"></i> Adjust
                        </button>
                        <button class="btn btn-sm btn-light rounded-pill px-3" onclick="viewHistory(${item.productId})">
                            <i class="fas fa-history me-1"></i> History
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

        // Update stats (Note: these stats are for the current page only in this simple implementation)
        $('#statTotalStock').text(totalStock);
        $('#statLowStock').text(lowStockCount);
        $('#statOutOfStock').text(outOfStockCount);

        renderPagination(result);
        $('#inventoryResultInfo').text(`Showing ${result.items.length} of ${result.totalCount} items`);
    }

    function renderPagination(result) {
        const pagination = $('#inventoryPagination');
        pagination.empty();

        if (result.totalPages <= 1) return;

        const prevDisabled = result.pageNumber === 1 ? 'disabled' : '';
        pagination.append(`<li class="page-item ${prevDisabled}"><a class="page-link" href="#" data-page="${result.pageNumber - 1}">Previous</a></li>`);

        for (let i = 1; i <= result.totalPages; i++) {
            const active = i === result.pageNumber ? 'active' : '';
            pagination.append(`<li class="page-item ${active}"><a class="page-link" href="#" data-page="${i}">${i}</a></li>`);
        }

        const nextDisabled = result.pageNumber === result.totalPages ? 'disabled' : '';
        pagination.append(`<li class="page-item ${nextDisabled}"><a class="page-link" href="#" data-page="${result.pageNumber + 1}">Next</a></li>`);
    }

    function loadCategories() {
        $.ajax({
            url: '/Admin/Inventory/GetCategories',
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const select = $('#inventoryCategorySelect');
                    response.data.forEach(function (cat) {
                        select.append(`<option value="${cat.id}">${cat.name}</option>`);
                    });
                }
            }
        });
    }

    window.openAdjustmentModal = function (productId, productName) {
        $('#adjProductId').val(productId);
        $('#adjustmentProductLabel').text(productName);
        $('#adjustmentForm')[0].reset();
        $('#adjustmentModal').modal('show');
    };

    window.viewHistory = function (productId) {
        $.ajax({
            url: '/Admin/Inventory/GetTransactions',
            type: 'GET',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    const tbody = $('#historyTableBody');
                    tbody.empty();
                    response.data.forEach(function (t) {
                        let typeLabel = '';
                        let colorClass = '';
                        switch (t.transactionType) {
                            case 1: typeLabel = 'Purchase'; colorClass = 'text-success'; break;
                            case 2: typeLabel = 'Sale'; colorClass = 'text-danger'; break;
                            case 3: typeLabel = 'Adjustment'; colorClass = 'text-info'; break;
                            case 4: typeLabel = 'Return'; colorClass = 'text-success'; break;
                            case 5: typeLabel = 'Damage'; colorClass = 'text-danger'; break;
                        }

                        const qtySign = t.quantity > 0 ? '+' : '';
                        const date = new Date(t.createdAt).toLocaleString();

                        tbody.append(`
                            <tr>
                                <td class="ps-3 small">${date}</td>
                                <td><span class="small fw-bold ${colorClass}">${typeLabel}</span></td>
                                <td class="text-end fw-bold">${qtySign}${t.quantity}</td>
                                <td class="small">${t.reason}</td>
                                <td class="small text-muted">${t.createdBy || 'System'}</td>
                            </tr>
                        `);
                    });
                    $('#historyModal').modal('show');
                }
            }
        });
    };

    function saveAdjustment() {
        const data = {
            ProductId: parseInt($('#adjProductId').val()),
            Quantity: parseInt($('#adjQuantity').val()),
            TransactionType: parseInt($('#adjType').val()),
            Reason: $('#adjReason').val(),
            ReferenceNumber: $('#adjRef').val()
        };

        const token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Admin/Inventory/Adjust',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: { 'RequestVerificationToken': token },
            success: function (response) {
                if (response.success) {
                    $('#adjustmentModal').modal('hide');
                    loadInventory();
                } else {
                    alert(response.message || 'Adjustment failed');
                }
            },
            error: function () {
                alert('An error occurred during adjustment');
            }
        });
    }
});
