angular.module("mainfuzzy")
    .constant("enums", {
        Degisken: { 'Ozdirenc': 1, 'Mukavemet': 2, 'Doygunluk': 3 },
        DegiskenList: [
            { Text: 'Özdirenç', Value: 1 },
            { Text: 'Mukavemet', Value: 2 },
            { Text: 'Doygunluk', Value: 3 },
        ],
        Mukavemet: { 'CokGevsek': 1, 'Gevsek': 2, 'Orta': 3, 'Siki': 4, 'Kaya': 5 },
        MukavemetList: [
            { Text: 'Çok Gevşek', Value: 1 },
            { Text: 'Gevşek', Value: 2 },
            { Text: 'Orta', Value: 3 },
            { Text: 'Sıkı', Value: 4 },
            { Text: 'Kaya', Value: 5 },
        ],
        Doygunluk: { 'SuyaDoygun': 1, 'SuyaVeGazaDoygun': 2, 'GazaDoygun': 3 },
        DoygunlukList: [
            { Text: 'Suya Doygun', Value: 1 },
            { Text: 'Suya Ve Gaza Doygun', Value: 2 },
            { Text: 'Gaza Doygun', Value: 3 },
        ],
        Esitlik: { 'EsitIse': 1, 'EsitDegilIse': 2 },
        EsitlikList: [
            { Text: 'Eşit İse (==)', Value: 1 },
            { Text: 'Eşit Değil İse (<>)', Value: 2 },
        ],
        Operator: { 'Ve': 1, 'Veya': 2, 'Yok' : 3 },
        OperatorList: [
            { Text: 'Ve (&&)', Value: 1 },
            { Text: 'Veya (||)', Value: 2 },
        ],
    });