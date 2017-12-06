angular.module("mainfuzzy", [])
    .controller("indexcontroller", function ($http, $scope) {
        $scope.Selam = function () {
            $http.get('/Home/Kaydet').then(function successCallback(response) {
                    if (response.data.Sonuc) {
                        debugger;
                        window.location.pathname = 'Mazeret/AktifMazeretlerim';
                    }
                    else {
                        $scope.hataMesajlari = [];
                        $scope.hataMesajlari.push(response.data.Mesaj);
                    }
                },
                    function errorCallback(response) {
                    });
            //$scope.mazeretKaydet = function (mazeret) {
            //    $http.post('/Mazeret/MazeretKaydet', mazeret).then(function successCallback(response) {
            //        if (response.data.Sonuc) {

            //            window.location.pathname = 'Mazeret/AktifMazeretlerim';
            //        }
            //        else {
            //            $scope.hataMesajlari = [];
            //            $scope.hataMesajlari.push(response.data.Mesaj);
            //        }
            //    },
            //        function errorCallback(response) {
            //        });
            //}
        }
    });
