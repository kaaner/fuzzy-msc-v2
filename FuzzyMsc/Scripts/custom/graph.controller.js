angular.module("mainfuzzy")
    .controller("graphcontroller", function ($http, $scope, enums, Upload,$timeout) {
        $scope.kumeListesi = [];
        $scope.panelExcelSec = false;
        $scope.panelOlcekSec = false;
        $scope.panelGrafik = false;
        $scope.olcek = { x: null, y: null };
        $scope.excel = {};


        $scope.KumeListesiGetir = function () {
            $http.get('/Graph/KumeListesiGetir').then(function successCallback(response) {
                if (response.data.Sonuc) {
                    $scope.kumeListesi = response.data.Nesne;
                }
                else {
                    $scope.hataMesajlari = [];
                    $scope.hataMesajlari.push(response.data.Mesaj);
                }
            },
                function errorCallback(response) {
                });
        }

        $scope.KumeListesiGetir();

        $scope.KuralSecimi = function (kural) {
            if (kural.KuralID == null || kural.KuralID == undefined) {
                $scope.panelDosyaSec = false;
            }
            else {
                $scope.kuralID = kural.KuralID;
                $http.get('/Graph/KuralGetir', { params: { kuralID: $scope.kuralID } }).then(function successCallback(response) {
                    if (response.data.Sonuc) {
                        $scope.kuralListesi = response.data.Nesne;
                        $scope.panelExcelSec = true;
                        $scope.panelOlcekSec = false;
                        $scope.panelGrafik = false;
                        $scope.olcek = { x: null, y: null };
                        $scope.excel = {};
                    }
                    else {
                        $scope.hataMesajlari = [];
                        $scope.hataMesajlari.push(response.data.Mesaj);
                    }
                },
                    function errorCallback(response) {
                    });
            }
        }

        $scope.ExcelSecimiVeGrafikOlustur = function (excel) {
            $scope.panelExcelSec = true;
            //$scope.panelOlcekSec = true;
            $scope.olcek = { x: null, y: null };
            var graph = { excel: $scope.excel, kuralID: $scope.kuralID, olcek: $scope.olcek };
            $http.post('/Graph/GraphOlustur', graph).then(function successCallback(response) {
                if (response.data.Sonuc) {
                    $scope.sonucDegerleri = response.data.Nesne;
                    $scope.panelGrafik = true;
                    $scope.GrafikCiz($scope.sonucDegerleri);
                    console.log("$scope.sonucDegerleri", $scope.sonucDegerleri);
                }
                else {
                    $scope.hataMesajlari = [];
                    $scope.hataMesajlari.push(response.data.Mesaj);
                }
            },
                function errorCallback(response) {
                });
        }

        $scope.OlcekSecimiVeGrafikOlustur = function (olcek) {
            $scope.panelExcelSec = true;
            $scope.panelOlcekSec = true;
            var graph = { excel: $scope.excel, kuralID: $scope.kuralID, olcek: $scope.olcek };
            $http.post('/Graph/GraphOlustur', graph).then(function successCallback(response) {
                if (response.data.Sonuc) {
                    $scope.sonucDegerleri = response.data.Nesne;
                    $scope.panelGrafik = true;
                    $scope.GrafikCiz($scope.sonucDegerleri);
                    console.log("$scope.sonucDegerleri", $scope.sonucDegerleri);
                }
                else {
                    $scope.hataMesajlari = [];
                    $scope.hataMesajlari.push(response.data.Mesaj);
                }
            },
                function errorCallback(response) {
                });
        }

        $scope.uploadFiles = function (file, errFiles) {
            $scope.f = file;
            $scope.errFile = errFiles && errFiles[0];
            if (file) {
                file.upload = Upload.upload({
                    url: '/Graph/UploadExcel',
                    data: { file: file }
                });

                file.upload.then(function (response) {
                    $timeout(function () {
                        $scope.excel = { adi: $scope.f.name, data: response.data.Nesne };                        
                    });
                }, function (response) {
                    if (response.status > 0)
                        $scope.errorMsg = response.status + ': ' + response.data;
                }, function (evt) {
                    file.progress = Math.min(100, parseInt(100.0 *
                        evt.loaded / evt.total));
                });
            }

        }

        $scope.GrafikCiz = function (chart) {
            Highcharts.chart('container', {
                chart: {
                    type: 'spline',
                    zoomType: 'xy',
                    panning: true,
                    panKey: 'shift'
                },
                title: {
                    text: 'Jeodezik Kesit Analizi, Burdur'
                },
                subtitle: {
                    text: 'Irregular time data in Highcharts JS'
                },
                xAxis: chart.xAxis
                ,
                yAxis: chart.yAxis
                ,

                legend: {
                    align: 'right',
                    verticalAlign: 'top',
                    layout: 'vertical',
                    x: 0,
                    y: 100
                },

                plotOptions: {
                    marker: {
                        enabled: true
                    },
                },

                annotations: chart.annotations,

                series: chart.series,

                exporting: {
                    sourceWidth: 5000,
                    sourceHeight: 2000,
                    // scale: 2 (default)
                    chartOptions: {
                        subtitle: null
                    }
                }

                //series: [{
                //    name: 'Winter 2012-2013',
                //    keys: ['x', 'y', 'vp', 'vs'],
                //    color: '#00FF00',
                //    marker: {
                //        symbol: 'circle'
                //    },
                //    // Define the data points. All series have a dummy year
                //    // of 1970/71 in order to be compared on the same x axis. Note
                //    // that in JavaScript, months start at 0 for January, 1 for February etc.
                //    data: [
                //        [0, 2, 1, 1],
                //        [1, 2.28, 1, 4],
                //        [2, 2.25, 1, 5],
                //        [3, 2.2, 1, 2],
                //        [4, 2.28],
                //        [5, 2.28],
                //        [6, 2.47],
                //        [7, 0.79],
                //        [8, 0.72],
                //        [9, 1.02],
                //        [10, 1.12],
                //        [11, 1.2],
                //        [12, 1.18],
                //        [13, 1.19],
                //        [14, 1.85],
                //        [15, 2.22],
                //        [16, 1.15],
                //        [17, 0]
                //    ]
                //}, {
                //    //type: 'line',
                //    name: 'Winter 2013-2014',
                //    color: Highcharts.getOptions().colors[7],
                //    marker: {
                //        enabled: false
                //    },
                //    data: [
                //        [0, 0, 1, 1],
                //        [1, 0.4, 1, 1],
                //        [3, 0.25, 1, 1],
                //        [3.5, 1.66, 1, 1],
                //        [4, 1.8],
                //        [5, 2.36],
                //        [5, 2.76],
                //        [5, 0.76],
                //        [5, 1.76],
                //        [6, 2.62],
                //        [7, 2.41],
                //        [8, 2.05],
                //        [9, 1.7],
                //        [10, 1.1],
                //        [11, 0]
                //    ]
                //}, {
                //    name: 'Çukur',
                //    showInLegend: false,
                //    tooltip: {
                //        headerFormat: '<b>{series.name}</b><br>',
                //        pointFormat: 'Çukur efem'
                //    },
                //    color: '#FFFF00',
                //    marker: {
                //        enabled: false
                //    },
                //    data: [
                //        [3, 0.25],
                //        [3.25, 1.5],
                //        [3.5, 1.66],
                //    ]
                //}, {
                //    name: 'Winter 2014-2015',
                //    marker: {
                //        enabled: false
                //    },
                //    color: Highcharts.getOptions().colors[3],
                //    data: [
                //        [0, 2],
                //        [1, 2.25],
                //        [2, 2.41],
                //        [3, 2.64],
                //        [4, 2.6],
                //        [5, 2.55],
                //        [6, 2.62],
                //        [7, 2.5],
                //        [8, 2.42],
                //        [9, 2.74],
                //        [10, 2.62],
                //        [11, 2.6],
                //        [12, 2.81],
                //        [13, 2.63],
                //        [14, 2.77],
                //        [15, 2.68],
                //        [16, 2.56],
                //        [17, 2.39],
                //        [18, 2.3],
                //        [19, 2],
                //        [20, 1.85],
                //        [21, 1.49],
                //        [22, 1.08],
                //        [23, 2.63],
                //        [24, 2.77],
                //        [25, 2.68],
                //        [26, 2.56],
                //        [27, 2.39],
                //        [28, 2.3],
                //        [29, 2],
                //        [30, 1.85],
                //        [31, 1.49],
                //        [32, 1.08]
                //    ]
                //}, {
                //    name: 'Çukur 2',
                //    color: Highcharts.getOptions().colors[3],
                //    showInLegend: false,
                //    marker: {
                //        enabled: false
                //    },
                //    data: [
                //        [25, 2.68],
                //        [26, 2.2],
                //        [27, 2.39],
                //    ]
                //}]
            });
        }

        
    });