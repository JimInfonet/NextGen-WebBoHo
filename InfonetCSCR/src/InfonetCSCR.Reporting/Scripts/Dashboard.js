function loadCharts() {
    $.ajax({
        url: getChartModelDataURL,
        method: "GET", success: function (data) {
            dataValue = jQuery.parseJSON(data);

            if (dataValue != null && dataValue["SalesChartDatas"].length == 0) {
                $('#noData').show();
                $('.chart-container').hide();
            } else {
                $('#noData').hide();
                $('.chart-container').show();

                loadSalesChart(dataValue["SalesChartDatas"]);
                loadTopCategoryChart(dataValue["TopCatChartData"]);
            }
        }
    });
}

// Define the formatMoney function

function loadSalesChart(dataValue) {
    var ctxy = document.getElementById("salesChart").getContext('2d');
    var chart = new Chart(ctxy, {
        type: 'line',
        data: {
            labels: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23],
            datasets: []
        },
        options: {
            scales: {
                xAxes: [{
                    scaleLabel: {
                        display: true,
                        labelString: 'Hour'
                    }
                }],
                yAxes: [{
                    ticks: {
                        callback: function (value) {
                            return formatMoney(value);
                        }
                    },
                    scaleLabel: {
                        display: true,
                        labelString: 'Dollar'
                    }
                }],
            },
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'SALES',
                position: 'top',
                fontFamily: 'Franklin Gothic Medium',
                fontSize: 16
            },
            plugins: {
                chartAreaBorder: {
                    borderColor: 'red',
                    borderWidth: 2,
                },
                datalabels: {
                    display: false
                },
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        var text = data.datasets[tooltipItem.datasetIndex].label;
                        var value = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];

                        if (text) {
                            text += ': ' + formatMoney(value);
                        }
                        return text;
                    }
                }
            }
        }
    });

    for (let i = 0; i < dataValue.length; i++) {
        chart.data.datasets.push({
            label: dataValue[i].Label,
            data: dataValue[i].Data,
            backgroundColor: transparentColorArray[i],
            borderColor: darkColorArray[i],
            fill: 1,
            borderWidth: 1
        });
    }

    chart.update();
}

function loadTopCategoryChart(dataValue) {
    var ctx = document.getElementById('categoryChart');
    const reducer = (accumulator, curr) => accumulator + curr;
    var total = dataValue.Data.reduce(reducer);

    myChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: dataValue.Labels,
            datasets: [{
                label: '# of Votes',
                data: dataValue.Data,
                backgroundColor: [
                    darkColorArray[1],
                    darkColorArray[12],
                    darkColorArray[13]
                ],
                borderColor: [
                    transparentColorArray[0],
                    transparentColorArray[1],
                    transparentColorArray[2],
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'TOP CATEGORIES',
                fontFamily: 'Franklin Gothic Medium',
                fontSize: 16
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        var label = data.labels[tooltipItem.index];
                        var value = data.datasets[0].data[tooltipItem.index];
                        if (label) {
                            label += ': ';
                        }

                        let percentage = ((value / total) * 100).toFixed(2) + '%';
                        label += percentage;
                        return label;
                    }
                }
            },
            plugins: {
                datalabels: {
                    backgroundColor: function (context) {
                        return context.dataset.backgroundColor;
                    },
                    borderColor: 'white',
                    borderRadius: 25,
                    borderWidth: 2,
                    color: 'white',
                    formatter: function (value) {
                        let percentage = ((value / total) * 100).toFixed(2) + '%';
                        return percentage;
                    },
                    display: function (context) {
                        var dataset = context.dataset;
                        var count = dataset.data.length;
                        var value = dataset.data[context.dataIndex];
                        return value > count * 1.5;
                    },
                    font: {
                        weight: 'bold'
                    },
                    padding: 6,
                }
            }
        }
    });
}
