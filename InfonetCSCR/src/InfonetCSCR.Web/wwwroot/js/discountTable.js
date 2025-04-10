let policies;
let discountTableData;

$(async () => {
  await initPolicy();
  initDiscountTable();
  await loadDiscountTableData();
});

let loadDiscountTableData = async () => {
  let url = "DiscountTable/GetAllDiscountTableData";
  await ajaxGetAsync(url, policies)
    .then(async (response) => {
      if (response) {
        discountTableData = response
        renderDataInDiscountTable();
      }
    })
    .catch(toastrError);
}

let initPolicy = async () => {
  const url = `DiscountTable/GetDiscountTablePolicies`;

  await ajaxGetAsync(url)
    .then(_ => policies = _)
    .catch(toastrError);
}

let renderDataInDiscountTable = () => {
  discountTableData.forEach((cellData) => {
    setCellValue(cellData.productDiscount, cellData.customerDiscount, cellData.discountPercentage)
  })
}

let setCellValue = (row, col, newValue) => {
  const tbody = document.querySelector("tbody");
  const targetRow = tbody.rows[row];
  const targetCell = targetRow.cells[col + 1];

  if (targetCell) {
    targetCell.textContent = newValue.toFixed(1);
  }
}

let insertDiscountTableData = async (discountTableData) => {
  let url = `DiscountTable/InsertDataOnDiscountTable`;
  await ajaxPostAsync(url, discountTableData)
    .then(async (response) => {
      await loadDiscountTableData();
    })
    .catch(toastrError);
}

let updateDiscountPercentage = async (discountTableData) => {
  let url = `DiscountTable/UpdateDiscountPercentage`;
  await ajaxPostAsync(url, discountTableData)
    .then(async (response) => {
      await loadDiscountTableData();
    })
    .catch(toastrError);
}

let deleteDiscount = async (discountTableData) => {
  let url = `DiscountTable/DeleteDiscount`;
  await ajaxPostAsync(url, discountTableData)
    .then(async (response) => {
      await loadDiscountTableData();
    })
    .catch(toastrError);
}

let initDiscountTable = () => {
  let numberOfRow = policies.isProductDiscount ? 99 : 0;
  let numberOfColumn = policies.isCustomerDiscount ? 99 : 0;

  // Generate headers for 0 to 99
  const theadRow = document.querySelector("thead tr");
  for (let i = 0; i <= numberOfColumn; i++) {
    const th = document.createElement("th");
    th.textContent = i;
    theadRow.appendChild(th);
  }

  if (numberOfColumn == 0) {
    for (let i = 1; i <= 99; i++) {
      const th = document.createElement("th");
      th.textContent = i;
      th.style.cssText = 'visibility: hidden !important; border: none !important';
      theadRow.appendChild(th);
    }

    const discountTableBody = document.getElementById("tableContainer");
    discountTableBody.style.overflowX = "hidden";
  }


  // Generate rows for 0 to 99
  const tbody = document.querySelector("tbody");
  for (let row = 0; row <= numberOfRow; row++) {
    const tr = document.createElement("tr");
    const th = document.createElement("th");
    th.textContent = row;
    tr.appendChild(th);

    for (let col = 0; col <= numberOfColumn; col++) {
      const td = document.createElement("td");
      td.setAttribute("contenteditable", "false");
      tr.appendChild(td);
    }

    tbody.appendChild(tr);
  }
  const cells = document.querySelectorAll('td');

  cells.forEach((cell) => {
    cell.addEventListener('click', () => {
      if (!cell.classList.contains('editing')) {
        cell.classList.add('editing');
        const input = document.createElement('input');
        input.type = 'number';
        input.value = cell.innerText;
        input.removeAttribute('step');
        input.addEventListener('blur', async () => {
          cell.classList.remove('editing');
          const newValue = parseFloat(input.value);
          const row = cell.parentElement.rowIndex - 1;
          const col = cell.cellIndex - 1;

          if (newValue > 100) {
            toastr.warning($("#discountsCannotBeMoreThan100").val());
            let data = discountTableData.find(obj => obj.productDiscount === row && obj.customerDiscount === col);
            if (data) {
              cell.innerText = ''
              cell.innerText = data.discountPercentage.toFixed(1);
            }
            else {
              cell.innerText = ''
            }
            return;
          }

          if (newValue < 0) {
            toastr.warning($("#discountsCannotBeNegative").val());
            let data = discountTableData.find(obj => obj.productDiscount === row && obj.customerDiscount === col);
            if (data) {
              cell.innerText = data.discountPercentage.toFixed(1);
            }
            else {
              cell.innerText = ''
            }
            return;
          }

          await processInput(row, col, newValue, cell);
        });
        cell.innerHTML = '';
        cell.appendChild(input);
        input.focus();
      }
    });

    cell.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') {
        const input = cell.querySelector('input');
        if (input) {
          input.blur();
          input.addEventListener('blur', async () => { });
        }
      }
    });
  });
}

let processInput = async (row, col, newValue, cell) => {
  let data = {
    productDiscount: row,
    customerDiscount: col,
    discountPercentage: newValue
  }

  if (!isNaN(newValue) && newValue !== 0) {
    cell.innerText = newValue.toFixed(1);
    let hasMatchingData = discountTableData.some(obj => obj.productDiscount === row && obj.customerDiscount === col);

    if (hasMatchingData) {
      await updateDiscountPercentage(data)
    }
    else {
      await insertDiscountTableData(data);
    }
  }
  else {
    await deleteDiscount(data)
    cell.innerText = '';
  }
}