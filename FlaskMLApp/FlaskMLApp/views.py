#!/usr/bin/env python
# -*- coding: utf-8 -*- 

"""
Routes and views for the flask application.
"""

from datetime import datetime
from flask import render_template
from FlaskMLApp import app
from flask import request
from flask import jsonify
import mobile_converter as mc 

@app.route('/')
@app.route('/home')
def home():
    """Renders the home page."""
    return render_template(
        'index.html',
        title='Home Page',
        year=datetime.now().year,
    )

@app.route('/contact')
def contact():
    """Renders the contact page."""
    return render_template(
        'contact.html',
        title='Contact',
        year=datetime.now().year,
        message='Your contact page.'
    )

@app.route('/about')
def about():
    """Renders the about page."""
    return render_template(
        'about.html',
        title='About',
        year=datetime.now().year,
        message='Your application description page.'
    )

@app.route('/calculateaccuracy', methods=['GET', 'POST'])
def calculateaccuracy():
    if request.method == 'GET':
        return render_template(
            'index.html',
            title='Home Page',
            year=datetime.now().year,
            message= request.get_json()
        )
        
    if request.method == 'POST':
        #mc.mlTest(request.json['path'])
        result = mc.mlAccuracy(request.json['path'], request.json['algorithm'], request.json['allFeatures'], request.json['features'], request.json['droppedFeatures'], request.json['target'])
        newResult = {
        'Exception': None,
        'Mesaj': 'Accuracy Bulundu',
        'Nesne': result,
        'Sonuc': True
        }

        return jsonify(newResult)

@app.route('/createandsavemodel', methods=['GET', 'POST'])
def createandsavemodel():
    if request.method == 'GET':
        return render_template(
            'index.html',
            title='Home Page',
            year=datetime.now().year,
            message= request.get_json()
        )
        
    if request.method == 'POST':
        
        result = mc.mlCreateAndSaveModel()
        newResult = {
        'Exception': None,
        'Mesaj': 'Model Oluşturuldu ve Kaydedildi. Dosyayı Proje Dizininde Bulabilirsiniz.',
        'Nesne': result,
        'Sonuc': True
        }

        return jsonify(newResult)





    #print(request.is_json)
    #content = request.get_json()
    ##print(content)
    #print('MUSTAFA ERDOĞMUŞ')
    #print('KAAN ER')
    #return render_template(
    #    'index.html',
    #    title='Home Page',
    #    year=datetime.now().year,
    #    message= request.get_json()
    #)