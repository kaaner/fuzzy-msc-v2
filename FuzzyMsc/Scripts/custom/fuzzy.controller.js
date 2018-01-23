angular.module("mainfuzzy")
    .controller("fuzzycontroller", function ($http, $scope, enums) {
        $scope.enums = enums;
        $scope.kumeAdi = "";
        $scope.ozdirencButonIptal = false;
        $scope.ozdirencButonGuncelle = false;
        $scope.ozdirencButonKaydet = true;
        $scope.toprakButonIptal = false;
        $scope.toprakButonGuncelle = false;
        $scope.toprakButonKaydet = true;
        $scope.kuralButonIptal = false;
        $scope.kuralButonGuncelle = false;
        $scope.kuralButonKaydet = true;
        $scope.panelToprak = false;
        $scope.panelKurallar = false;

        $scope.zeminList = [];
        $scope.ozdirencList = [{
            adi: "Düşük",
            minDeger: 0,
            maxDeger: 30
        }, {
            adi: "Orta",
            minDeger: 20,
            maxDeger: 50
        }, {
            adi: "Yüksek",
            minDeger: 50,
            maxDeger: 70
        }, {
            adi: "Çok Yüksek",
            minDeger: 40,
            maxDeger: 80
        }];
        $scope.toprakList = [{
            adi: "Kil",
            minDeger: 0,
            maxDeger: 30
        }, {
            adi: "Silt",
            minDeger: 20,
            maxDeger: 50
        }, {
            adi: "Kum",
            minDeger: 50,
            maxDeger: 70
        }, {
            adi: "Çakıl",
            minDeger: 40,
            maxDeger: 80
        }];

        $scope.kuralList = [];
        $scope.sonucDegerleri = [];
        $scope.preKuralList = [];

        $scope.Test = function () {
            $http.get('/Fuzzy/Test').then(function successCallback(response) {
                if (response.data.Sonuc) {
                    alert(response.data);
                    //window.location.pathname = 'Mazeret/AktifMazeretlerim';
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

        $scope.Ekle = function (item) {
            $scope.zeminList.push({
                ozdirenc: item.ozdirenc,
                mukavemet: item.mukavemet,
                doygunluk: item.doygunluk
            });
        }

        $scope.Sonuclar = function (zeminList) {
            $http.post('/Fuzzy/Sonuclar', zeminList).then(function successCallback(response) {
                if (response.data.Sonuc) {
                    $scope.sonucDegerleri = response.data.Nesne;
                    //window.location.pathname = 'Mazeret/AktifMazeretlerim';
                }
                else {
                    $scope.hataMesajlari = [];
                    $scope.hataMesajlari.push(response.data.Mesaj);
                }
            },
                function errorCallback(response) {
                });
        }

        //Ozdirenc Islemleri START
        $scope.OzdirencEkle = function (item) {
            $scope.ozdirencList.push({
                adi: item.adi,
                minDeger: item.minDeger,
                maxDeger: item.maxDeger
            });
            $scope.ozdirenc = {};
        }

        $scope.OzdirencGuncelle = function (item) {
            $scope.ozdirencList[item.$index] = item;
            $scope.ozdirenc = {};
            $scope.ozdirencButonIptal = false;
            $scope.ozdirencButonGuncelle = false;
            $scope.ozdirencButonKaydet = true;
        }

        $scope.OzdirencSil = function ($index) {
            $scope.ozdirencList.splice($index, 1);
        }

        $scope.OzdirencDuzenle = function (item, $index) {
            $scope.ozdirencButonIptal = true;
            $scope.ozdirencButonGuncelle = true;
            $scope.ozdirencButonKaydet = false;
            $scope.ozdirenc = angular.copy(item);
            $scope.ozdirenc.$index = $index;
        }

        $scope.OzdirencIptal = function () {
            $scope.ozdirenc = {};
            $scope.ozdirencButonIptal = false;
            $scope.ozdirencButonGuncelle = false;
            $scope.ozdirencButonKaydet = true;
        }

        $scope.OzdirencKaydet = function (ozdirencList) {
            $scope.panelToprak = true;
        }
        //Ozdirenc Islemleri END

        //Toprak Islemleri START
        $scope.ToprakEkle = function (item) {
            $scope.toprakList.push({
                adi: item.adi,
                minDeger: item.minDeger,
                maxDeger: item.maxDeger
            });
            $scope.toprak = {};
        }

        $scope.ToprakGuncelle = function (item) {
            $scope.toprakList[item.$index] = item;
            $scope.toprak = {};
            $scope.toprakButonIptal = false;
            $scope.toprakButonGuncelle = false;
            $scope.toprakButonKaydet = true;
        }

        $scope.ToprakSil = function ($index) {
            $scope.toprakList.splice($index, 1);
        }

        $scope.ToprakDuzenle = function (item, $index) {
            $scope.toprakButonIptal = true;
            $scope.toprakButonGuncelle = true;
            $scope.toprakButonKaydet = false;
            $scope.toprak = angular.copy(item);
            $scope.toprak.$index = $index;
        }

        $scope.ToprakIptal = function () {
            $scope.toprak = {};
            $scope.toprakButonIptal = false;
            $scope.toprakButonGuncelle = false;
            $scope.toprakButonKaydet = true;
        }

        $scope.ToprakKaydet = function (toprakList) {
            $scope.panelKurallar = true;
        }
        //Toprak Islemleri END

        //Kural Islemleri START
        $scope.KuralEkle = function (kural) {
            $scope.kuralList.push({
                text: "Özdirenç Değeri " + kural.ozdirenc + " İse Toprak " + kural.toprak + " Olur.",
                kural: kural
            });
            $scope.kural = {};
        }

        $scope.KuralGuncelle = function (kural) {
            debugger;
            var kuralItem = {
                text: "Özdirenç Değeri " + kural.ozdirenc + " İse Toprak " + kural.toprak + " Olur.",
                kural: kural
            };
            $scope.kuralList[kural.$index] = kuralItem;
            $scope.kural = {};
            $scope.kuralButonIptal = false;
            $scope.kuralButonGuncelle = false;
            $scope.kuralButonKaydet = true;
        }

        $scope.KuralSil = function ($index) {
            $scope.kuralList.splice($index, 1);
        }

        $scope.KuralDuzenle = function (item, $index) {
            $scope.kuralButonIptal = true;
            $scope.kuralButonGuncelle = true;
            $scope.kuralButonKaydet = false;
            $scope.kural = angular.copy(item.kural);
            $scope.kural.$index = $index;
        }

        $scope.KuralIptal = function () {
            $scope.kural = {};
            $scope.kuralButonIptal = false;
            $scope.kuralButonGuncelle = false;
            $scope.kuralButonKaydet = true;
        }

        $scope.KuralKaydet = function (kuralList) {
            $scope.panelKurallar = true;
        }

        $scope.OtomatikTanimla = function () {
            if ($scope.ozdirencList.length == $scope.toprakList.length) {
                for (var i = 0; i < $scope.ozdirencList.length; i++) {
                    var kural = { ozdirenc: $scope.ozdirencList[i].adi, toprak: $scope.toprakList[i].adi };
                    $scope.KuralEkle(kural);
                }
            }
        }
        //Kural Islemleri END

        $scope.KumeKaydet = function (kuralList) {
            var kuralKume = { kumeAdi: $scope.kumeAdi, kuralList: kuralList, ozdirencList: $scope.ozdirencList, toprakList: $scope.toprakList };
            $http.post('/Fuzzy/KumeKaydet', kuralKume).then(function successCallback(response) {
                if (response.data.Sonuc) {
                    $scope.sonucDegerleri = response.data.Nesne;
                    //window.location.pathname = 'Mazeret/AktifMazeretlerim';
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
