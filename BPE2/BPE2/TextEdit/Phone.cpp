#include "pch.h"
#include "Phone.h"
using namespace TextEdit;
Phone::Phone(String^ model,
	String^ producer,
	String^ formFactor,
	String^ typeOfNetwork,
	String^ modelYear,
	String^ countOfCards,
	String^ price)
{
	this->model = model;
	this->producer = producer;
	this->formFactor = formFactor;
	this->typeOfNetwork = typeOfNetwork;
	this->modelYear = modelYear;
	this->countOfCards = countOfCards;
	this->price = price;
	
}

Phone::Phone(){}
