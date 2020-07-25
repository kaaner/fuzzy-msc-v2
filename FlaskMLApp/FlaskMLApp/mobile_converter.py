import pickle

import pandas as pd
import numpy as np
import coremltools
import enum
import unicodedata

from coremltools.models import MLModel
from xgboost.sklearn import XGBClassifier
from sklearn.svm import LinearSVC, SVC
from sklearn.ensemble import *
from sklearn import tree
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score as skas
from coremltools.converters import sklearn as sk
from coremltools.converters import xgboost as xgb
from flask import jsonify

globalModel = {'model':None,'path':'', 'algorithm':0,'allFeatures':None,'features':None,'droppedFeatures':None,'target':'','accuracy':None, 'name':'', 'converter': None}

def mlAccuracy(path, algorithm,allFeatures,features,droppedFeatures,targetValue):
    globalModel['converter'] = Converters.sklearn

    classifier = int(algorithm)
    features = unicodeToString(features)
    allFeatures = unicodeToString(allFeatures)
    droppedFeatures = unicodeToString(droppedFeatures)
    targetValue = unicodedata.normalize('NFKD', targetValue).encode('ascii', 'ignore')
    df = pd.read_csv(path)
    
    target = df[targetValue]
    df2 = df.drop(droppedFeatures, axis=1)
    X_train, X_test, y_train, y_test = train_test_split(df2, target, test_size=0.3, shuffle=True, random_state=123456)


    if classifier == Classifiers.RandomForestClassifier:
        cls = RandomForestClassifier()
    elif classifier == Classifiers.DecisionTreeClassifier:
        cls = tree.DecisionTreeClassifier()
    elif classifier == Classifiers.ExtraTreesClassifier:
        cls = ExtraTreesClassifier()
    elif classifier == Classifiers.AdaBoostClassifier:
        cls = AdaBoostClassifier()
    elif classifier == Classifiers.BaggingClassifier:
        cls = BaggingClassifier()
    elif classifier == Classifiers.GradientBoostingClassifier:
        cls = GradientBoostingClassifier()
    elif classifier == Classifiers.VotingClassifier:
        cls = VotingClassifier()
    elif classifier == Classifiers.XGBClassifier:
        cls = XGBClassifier()
        globalModel['converter'] = Converters.xgboost        
    elif classifier == Classifiers.LinearSVC:
        cls = LinearSVC()
    else:
        cls = SVC(gamma=1)
    #print y_train[0:10]

    model = cls.fit(X_train, y_train)
    predicted = cls.predict(X_test)
    accuracy = skas(y_test, predicted)
    name = type(cls).__name__.lower()
    
    globalModel['model'] = model
    globalModel['path'] = path
    globalModel['algorithm'] = algorithm
    globalModel['allFeatures'] = allFeatures
    globalModel['features'] = features
    globalModel['droppedFeatures'] = droppedFeatures
    globalModel['target'] = targetValue
    globalModel['accuracy'] = accuracy
    globalModel['name'] = name

    return accuracy

#Static Code
#def mlTest(path):
#    features = ["ID", "Gender", "Age", "AccX", "AccY", "AccZ", "GyroX",
#    "GyroY", "GyroZ"]
#    df = pd.read_csv(path)
#    target = df["ID"]
#    df2 = df.drop("ID", axis=1)
#    X_train, X_test, y_train, y_test = train_test_split(df2, target,
#    test_size=0.3, shuffle=True, random_state=123456)
#    rf = RandomForestClassifier()
#    model = rf.fit(X_train, y_train)
#    predicted = rf.predict(X_test)
#    coreml_model = coremltools.converters.sklearn.convert(model, ["Gender",
#    "Age","GyroX", "GyroY",
#                  "GyroZ","AccX", "AccY", "AccZ"], "ID")
#    coreml_model.save('knn.mlmodel')
#    loaded_model = MLModel('knn.mlmodel')

def mlCreateAndSaveModel():
    model = globalModel['model']
    features = globalModel['features']
    target = globalModel['target']
    if globalModel['converter'] == Converters.sklearn:
        coreml_model = coremltools.converters.sklearn.convert(model, features, target)
    else:
        coreml_model = coremltools.converters.xgboost.convert(model, features, target)

    coreml_model.save(globalModel['name'] + '.mlmodel')
    loaded_model = MLModel(globalModel['name'] + '.mlmodel')

    return True

def unicodeToString(unicodeArray):
    stringArray = []

    for x in unicodeArray:
        stringItem = unicodedata.normalize('NFKD', x).encode('ascii', 'ignore')
        stringArray.append(stringItem)
    
    return stringArray

class Classifiers(enum.Enum):
    RandomForestClassifier = 1
    DecisionTreeClassifier = 2
    ExtraTreesClassifier = 3
    AdaBoostClassifier = 4
    BaggingClassifier = 5
    GradientBoostingClassifier = 6
    VotingClassifier = 7
    XGBClassifier = 8
    LinearSVC = 9
    SVC = 10

class Converters(enum.Enum):
    sklearn = 1
    xgboost = 2

    #Classifiers = enum(RandomForestClassifier = 1, DecisionTreeClassifier = 2, GradientBoostingClassifier = 3)
    #Classifiers = enum('RandomForestClassifier', 'DecisionTreeClassifier', 'GradientBoostingClassifier')
