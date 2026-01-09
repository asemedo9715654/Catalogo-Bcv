$(document).ready(function () {
    const $searchInput = $('#globalSearchInput');
    const $searchResults = $('#searchResults');
    let debounceTimer;

    $searchInput.on('input', function () {
        const query = $(this).val();
        
        clearTimeout(debounceTimer);
        
        if (query.length < 2) {
            $searchResults.hide().empty();
            return;
        }

        debounceTimer = setTimeout(function () {
            $.get('/Catalog/Search', { query: query }, function (data) {
                $searchResults.empty();

                if (data.databases.length === 0 && data.tables.length === 0 && data.columns.length === 0) {
                    $searchResults.append('<div class="list-group-item">Nenhum resultado encontrado</div>');
                } else {
                    if (data.databases.length > 0) {
                        $searchResults.append('<div class="list-group-item list-group-item-light fw-bold">Bases de Dados</div>');
                        data.databases.forEach(db => {
                            $searchResults.append(`<a href="/Catalog/Details/${db.id}" class="list-group-item list-group-item-action">${db.name} <small class="text-muted">(${db.server})</small></a>`);
                        });
                    }

                    if (data.tables.length > 0) {
                        $searchResults.append('<div class="list-group-item list-group-item-light fw-bold">Tabelas</div>');
                        data.tables.forEach(table => {
                            $searchResults.append(`<a href="/Catalog/TableDetails/${table.id}" class="list-group-item list-group-item-action">${table.schema}.${table.name} <small class="text-muted">em ${table.databaseName}</small></a>`);
                        });
                    }

                    if (data.columns.length > 0) {
                        $searchResults.append('<div class="list-group-item list-group-item-light fw-bold">Colunas</div>');
                        data.columns.forEach(col => {
                            $searchResults.append(`<a href="/Catalog/TableDetails/${col.id}" class="list-group-item list-group-item-action">${col.columnName} <small class="text-muted">em ${col.databaseName}.${col.tableName}</small></a>`);
                        });
                    }
                }
                
                $searchResults.show();
            }).fail(function() {
                 $searchResults.empty().append('<div class="list-group-item text-danger">Erro ao pesquisar</div>').show();
            });
        }, 300);
    });

    // Close search results when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#globalSearchInput').length && !$(e.target).closest('#searchResults').length) {
            $searchResults.hide();
        }
    });
    
    // Show results again if input is focused and has value
    $searchInput.on('focus', function() {
        if ($(this).val().length >= 2 && $searchResults.children().length > 0) {
            $searchResults.show();
        }
    });
});
