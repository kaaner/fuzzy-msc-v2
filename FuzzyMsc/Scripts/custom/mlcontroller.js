angular.module("mainfuzzy")
    .controller("mlcontroller", function ($http, $scope, enums, Upload, $timeout, $translate, $rootScope) {
        $scope.enums = enums;
        $scope.excel = {};
        $scope.excelError = false;

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
                        $scope.excel = { adi: $scope.f.name, data: response.data.Nesne.data, path: response.data.Nesne.path };
                    });
                }, function (response) {
                    if (response.status > 0)
                        $scope.errorMsg = response.status + ': ' + response.data;
                }, function (evt) {
                    file.progress = Math.min(100, parseInt(100.0 *
                        evt.loaded / evt.total));
                });
                $scope.excelError = false;
            }

        }

        $scope.Sonuclar = function (excel, algorithm) {
            var datas = { excel: excel, algorithm: algorithm };
            $http.post('/MachineLearning/Test', datas).then(function successCallback(response) {
                if (response.data.Sonuc) {
                    $scope.sonucDegerleri = response.data.Nesne;
                    window.location.href = '/Graph/Cizim';
                }
                else {
                    $scope.hataMesajlari = [];
                    $scope.hataMesajlari.push(response.data.Mesaj);
                }
            },
                function errorCallback(response) {
                });
        }

    });
