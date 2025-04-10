let adjustSDEPanelHeight = (row) => {
  let mediaQuery = window.matchMedia('(min-width: 1366px)');

  if (!mediaQuery.matches) return;

  let maxHeight = 0;

  row.forEach(col => {
    let grid = $(`#${col}-table`);
    grid.height('auto') ;
    maxHeight = Math.max(grid.height(), maxHeight);
  });

  row.forEach(col => {
    let grid = $(`#${col}-table`);
    grid.height(maxHeight);
  });
}

let enableOrDisableGridEditDeleteButtons = (gridName, isDisable) => {
  $(`#${gridName}-btn-edit`).prop('disabled', isDisable);
  $(`#${gridName}-btn-delete`).prop('disabled', isDisable);
}
