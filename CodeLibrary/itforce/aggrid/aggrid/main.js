function myFunction(){
  dotNetMessage.show('123456');
}
function LoadData(){
  var data = dotNetMessage.getData();
  gridOptions.api.setRowData(JSON.parse(data));
}
var gridOptions = {
  columnDefs: [
    {
      headerName: 'Athlete',
      field: 'athlete',
      minWidth: 180,
      headerCheckboxSelection: true,
      headerCheckboxSelectionFilteredOnly: true,
      checkboxSelection: true,      
    },
    { field: 'age' },
    { field: 'country', 
      minWidth: 150,
      cellEditor: 'agSelectCellEditor',
      cellEditorParams: {
        values: ['Porsche', 'Toyota', 'Ford', 'AAA', 'BBB', 'CCC'],
      },
      rowGroup: true, hide: true,
    },
    { field: 'year' },
    { field: 'date', minWidth: 150 },
    { field: 'sport', cellEditor: 'agLargeTextCellEditor', minWidth: 150 },
    { field: 'gold' },
    { field: 'silver' },
    { field: 'bronze' },
    { field: 'total' },
  ],
  defaultColDef: {
    flex: 1,
    minWidth: 110,
    filter: true,
    sortable: true,
    editable: true,
    resizable: true,
  },
  editType: 'fullRow',
  singleClickEdit: true,
  //autoGroupColumnDef: {
  //  minWidth: 200,
  //},
  enableRangeSelection: true,
  animateRows: true,  
  //suppressRowClickSelection: true,
  rowSelection: 'multiple',
};



function onQuickFilterChanged() {
  gridOptions.api.setQuickFilter(document.getElementById('quickFilter').value);
}

// setup the grid after the page has finished loading
document.addEventListener('DOMContentLoaded', function () {
  var gridDiv = document.querySelector('#myGrid');
  new agGrid.Grid(gridDiv, gridOptions);

  agGrid
    .simpleHttpRequest({
      url:
        'http://localhost:18002/rba-backend-item-api/aggrid',
    })
    .then(function (data) {
      gridOptions.api.setRowData(data);
    });
});