angular.module("mainfuzzy")
    .controller("mlcontroller", function ($http, $scope, enums, Upload, $timeout, $translate, $rootScope, readFileData, SweetAlert) {
        $scope.enums = enums;
        $scope.excel = {};
        $scope.excelError = false;
        $scope.panelMain = true;
        $scope.panelFeatures = false;
        $scope.panelAccuracy = false;
        $scope.fileDataObj = {};

        $scope.features = [];
        $scope.allFeatures = [];
        $scope.droppedFeatures = [];
        $scope.accuracy = 0.0;

        $scope.uploadFile = function () {
            if ($scope.fileContent) {
                var file = $scope.filePath;
                $scope.fileDataObj = readFileData.processData($scope.fileContent);

                $scope.fileData = JSON.stringify($scope.fileDataObj);

                file.upload = Upload.upload({
                    url: '/MachineLearning/GetFullPath',
                    data: { file: file }
                });

                file.upload.then(function (response) {
                    $timeout(function () {
                        $scope.fullPath = response.data.Nesne;
                        $scope.panelFeatures = true;
                        for (var i = 0; i < $scope.fileDataObj[0].length; i++) {
                            $scope.features.push($scope.fileDataObj[0][i]);
                            $scope.allFeatures.push($scope.fileDataObj[0][i]);
                        }

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
        
        $scope.moveToFeatures = function (item) {
            $scope.features.push(item);
            $scope.droppedFeatures.splice($scope.droppedFeatures.indexOf(item), 1);
        };

        $scope.moveToDroppedFeatures = function (item) {
            $scope.droppedFeatures.push(item);
            $scope.features.splice($scope.features.indexOf(item), 1);
        };

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
                if (!$scope.excelError) {
                    $scope.panelFeatures = true;
                }
            }

        }

        $scope.Sonuclar = function (algorithm, target) {
            var datas = { path: $scope.fullPath, algorithm: algorithm, target: target, allFeatures: $scope.allFeatures, features: $scope.features, droppedFeatures: $scope.droppedFeatures };
            $http.post('/MachineLearning/Test', datas).then(function successCallback(response) {
                if (response.data.Sonuc) {
                    //$scope.sonucDegerleri = response.data.Nesne;
                    $scope.panelAccuracy = true;
                    $scope.panelMain = false;
                    $scope.accuracy = response.data.Nesne;
                }
                else {
                    $scope.hataMesajlari = [];
                    $scope.hataMesajlari.push(response.data.Mesaj);
                }
            },
                function errorCallback(response) {
                });
        }

        $scope.Reset = function () {
            $scope.panelAccuracy = false;
            $scope.panelMain = true;
        }

        $scope.CreateAndSaveModel = function () {
            $http.post('/MachineLearning/CreateAndSaveModel').then(function successCallback(response) {
                if (response.data.Sonuc) {
                    console.log(response.data)
                    SweetAlert.swal({
                        title: "Başarılı",
                        text: response.data.Mesaj,
                        type: "success",
                        showCancelButton: false,
                        //confirmButtonColor: "#DD6B55",
                        confirmButtonText: "TAMAM",
                        closeOnConfirm: true
                    },
                        function () {
                        });
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
