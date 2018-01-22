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
        $scope.panelToprak = false;
        $scope.panelKurallar = false;

        $scope.zeminList = [];
        $scope.ozdirencList = [];
        $scope.toprakList = [];
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

        $scope.OperatorIleBagla = function (kural, operator) {
            var text = "";
            var preKuralItem = { text: "", kural: {}};

            if (kural.degisken == $scope.enums.Degisken.Ozdirenc) {
                text = $scope.enums.DegiskenList[0].Text + " " + $scope.enums.EsitlikList[kural.esitlik - 1].Text + " " + kural.ozdirenc
            } else if (kural.degisken == enums.Degisken.Mukavemet) {
                text = $scope.enums.DegiskenList[1].Text + " " + $scope.enums.EsitlikList[kural.esitlik - 1].Text + " " + $scope.enums.MukavemetList[kural.mukavemet - 1].Text
            } else if (kural.degisken == enums.Degisken.Doygunluk) {
                text = $scope.enums.DegiskenList[2].Text + " " + $scope.enums.EsitlikList[kural.esitlik - 1].Text + " " + $scope.enums.DoygunlukList[kural.doygunluk - 1].Text
            }
            if (operator === $scope.enums.Operator.Yok) {
                preKuralItem = { text: text, kural: kural };
            } else {
                kural.operator = operator;
                preKuralItem = { text: $scope.enums.OperatorList[operator - 1].Text + " " + text, kural: kural };
            }
            $scope.preKuralList.push(preKuralItem);
            $scope.kural = {esitlik:kural.esitlik};
        }
        
        $scope.PreKuralSil = function ($index) {
            $scope.preKuralList.splice($index, 1);
        }

        $scope.PreKuralDuzenle = function (item, $index) {
            $scope.kural = angular.copy(item);
            $scope.kural.$index = $index;
        }

        $scope.KuralKaydet = function (preKuralList, sonuc) {
            $scope.kuralListItem = { text: "", kurallar: [] };
            for (var i = 0; i < preKuralList.length; i++) {
                $scope.kuralListItem.text = $scope.kuralListItem.text + " " + preKuralList[i].text;
                $scope.kuralListItem.kurallar.push(preKuralList[i].kural);
            }
            $scope.kuralListItem.text = $scope.kuralListItem.text + " İse Toprak " + sonuc;
            $scope.kuralListItem.sonuc = sonuc;
            $scope.kuralList.push($scope.kuralListItem);
            $scope.kural = {};
            $scope.preKuralList = [];
        }

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
