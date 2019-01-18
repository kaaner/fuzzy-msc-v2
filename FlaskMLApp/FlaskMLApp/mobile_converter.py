import pickle

import pandas as pd
import numpy as np
import coremltools
import enum

from coremltools.models import MLModel
from sklearn.ensemble import *
from sklearn import tree
from sklearn.model_selection import train_test_split
from coremltools.converters import sklearn as sk
from flask import jsonify

def mlPredict(path, algorithm):
    default_value = 1
    classifier = int(algorithm)
    features = ["ID", "Gender", "Age", "AccX", "AccY", "AccZ", "GyroX", "GyroY", "GyroZ"]
    df = pd.read_csv(path)
    
    target = df["ID"]
    df2 = df.drop("ID", axis=1)
    X_train, X_test, y_train, y_test = train_test_split(df2, target, test_size=0.3, shuffle=True, random_state=123456)
    firstCondition = next( (x for x in algorithms if x['ID'] == classifier), default_value)
    if classifier == Classifiers.RandomForestClassifier:
        cls = RandomForestClassifier()
    elif classifier == Classifiers.DecisionTreeClassifier:
        cls = tree.DecisionTreeClassifier()
    else:
        cls = GradientBoostingClassifier()
    model=cls.fit(X_train, y_train)
    predicted = cls.predict(X_test)
    
    return jsonify({'predicted': predict, 'model': model})

def mlCreateAndSaveModel(model, target):
    coreml_model = sk.convert(model, ["Gender", "Age","GyroX", "GyroY",
                  "GyroZ","AccX", "AccY", "AccZ"], "ID")
    coreml_model.save('knn.mlmodel')
    loaded_model = MLModel('linear_model.mlmodel')

class Classifiers(enum.Enum):
    RandomForestClassifier = 1
    DecisionTreeClassifier = 2
    GradientBoostingClassifier = 3
