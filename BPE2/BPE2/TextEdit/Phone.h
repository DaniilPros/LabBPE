#pragma once
using namespace Platform;
using namespace std;
namespace TextEdit
{
	[Windows::UI::Xaml::Data::Bindable]
	public ref class Phone sealed
	{
	public:
		Phone();
		Phone(String^ model,
			String^ producer,
			String^ formFactor,
			String^ typeOfNetwork,
			String^ modelYear,
			String^ countOfCards,
			String^ pric);

		property String^ model;
		property String^ producer;
		property String^ formFactor;
		property String^ typeOfNetwork;
		property String^ modelYear;
		property String^ countOfCards;
		property String^ price;
		

	};
}


