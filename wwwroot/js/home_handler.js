document.addEventListener('DOMContentLoaded', function () {
    // Hàm để build URL với tất cả filters và page
    function applyFilters(page = 1) {
        const params = new URLSearchParams();
        const currentUrl = new URL(window.location);

        // Giữ lại keyword nếu có
        if (currentUrl.searchParams.get('keyword')) {
            params.set('keyword', currentUrl.searchParams.get('keyword'));
        }

        // Giữ lại category selection nếu có
        if (currentUrl.searchParams.get('parentCategoryId.Id')) {
            params.set('parentCategoryId.Id', currentUrl.searchParams.get('parentCategoryId.Id'));
        }
        if (currentUrl.searchParams.get('categoryId.Id')) {
            params.set('categoryId.Id', currentUrl.searchParams.get('categoryId.Id'));
        }

        // Publisher filters
        const selectedPublishers = [];
        document.querySelectorAll('.publisher-filter:checked').forEach(function (checkbox) {
            selectedPublishers.push(checkbox.getAttribute('data-publisher-id'));
        });
        selectedPublishers.forEach(function (id) {
            params.append('publisherId', id);
        });

        // Price range filters
        const selectedPriceRanges = [];
        document.querySelectorAll('.price-filter:checked').forEach(function (checkbox) {
            selectedPriceRanges.push(checkbox.getAttribute('data-price-range'));
        });
        selectedPriceRanges.forEach(function (range) {
            params.append('rangePrice', range);
        });

        // Thêm page vào params
        params.set('page', page);

        // Navigate to new URL
        window.location.href = '/Customer/Books?' + params.toString();
    }

    // Event listeners for filters
    document.querySelectorAll('.price-filter, .publisher-filter').forEach(function (filter) {
        filter.addEventListener('change', () => applyFilters());
    });

    // Category link handlers
    document.querySelectorAll('.category-filter-link').forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            const categoryId = this.getAttribute('data-category-id');
            const parentId = this.getAttribute('data-parent-id');

            const params = new URLSearchParams();

            // Preserve current filters
            const currentUrl = new URL(window.location);
            if (currentUrl.searchParams.get('keyword')) {
                params.set('keyword', currentUrl.searchParams.get('keyword'));
            }

            // Set category
            params.set('categoryId.Id', categoryId);
            params.set('parentCategoryId.Id', parentId);

            // Preserve other filters
            document.querySelectorAll('.publisher-filter:checked').forEach(function (checkbox) {
                params.append('publisherId', checkbox.getAttribute('data-publisher-id'));
            });

            document.querySelectorAll('.price-filter:checked').forEach(function (checkbox) {
                params.append('rangePrice', checkbox.getAttribute('data-price-range'));
            });

            // Preserve page if exists
            if (currentUrl.searchParams.get('page')) {
                params.set('page', currentUrl.searchParams.get('page'));
            }

            window.location.href = '/Customer/Books?' + params.toString();
        });
    });

    // Pagination link handlers
    document.querySelectorAll('.page-link').forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            // Ignore disabled links
            if (this.parentElement.classList.contains('disabled')) {
                return;
            }

            const currentUrl = new URL(window.location);
            let currentPage = parseInt(currentUrl.searchParams.get('page') || '1');

            // Determine the page number to navigate to
            let targetPage;
            if (this.textContent === 'Trước') {
                targetPage = currentPage - 1;
            } else if (this.textContent === 'Sau') {
                targetPage = currentPage + 1;
            } else {
                targetPage = parseInt(this.textContent);
            }

            // Call applyFilters with the target page
            applyFilters(targetPage);
        });
    });

    // Clear filters button (optional)
    function addClearFiltersButton() {
        const clearButton = document.createElement('button');
        clearButton.textContent = 'Xóa tất cả bộ lọc';
        clearButton.className = 'btn btn-outline-secondary btn-sm mt-2';
        clearButton.addEventListener('click', function () {
            window.location.href = '/Customer/Books';
        });

        const categoriesSection = document.querySelector('.categories-section');
        if (categoriesSection) {
            categoriesSection.appendChild(clearButton);
        }
    }

    // Initialize clear filters button
    addClearFiltersButton();
});