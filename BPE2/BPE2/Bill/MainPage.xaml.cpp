//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"

using namespace Bill;

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

MainPage::MainPage()
{
	InitializeComponent();
}


void Bill::MainPage::IdTextBox_TextChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::TextChangedEventArgs^ e)
{
	auto text = dynamic_cast<TextBox^>(sender)->Text;
	auto pos = dynamic_cast<TextBox^>(sender)->SelectionStart;
	for (int i = 0; i < text->Length();i++)
	{
		if (!isdigit(text->Data()[i]))
		{
			auto correct = std::wstring(text->Data());
			correct.erase(i, 1);
			text = ref new String(correct.c_str());
			if (i <= pos) pos--;
		}
	}
	dynamic_cast<TextBox^>(sender)->Text = text;
	dynamic_cast<TextBox^>(sender)->SelectionStart = pos;
	
}


void Bill::MainPage::NameTextBox_TextChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::TextChangedEventArgs^ e)
{
	auto text = dynamic_cast<TextBox^>(sender)->Text;
}


void Bill::MainPage::PriceTextBox_TextChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::TextChangedEventArgs^ e)
{
	auto text = dynamic_cast<TextBox^>(sender)->Text;
	auto pos = dynamic_cast<TextBox^>(sender)->SelectionStart;
	std::pair<std::wstring, int> pair = DeleteSymbols(text->Data(), '.', pos);
	pos = pair.second;
	text = ref new String(pair.first.c_str());
	for (int i = 0; i < text->Length(); i++)
	{
		if (!isdigit(text->Data()[i])&& text->Data()[i]!='.')
		{
			auto correct = std::wstring(text->Data());
			correct.erase(i, 1);
			text = ref new String(correct.c_str());
			if (i <= pos) pos--;
		}
	}
	dynamic_cast<TextBox^>(sender)->Text = text;
	dynamic_cast<TextBox^>(sender)->SelectionStart = pos;
}


void Bill::MainPage::CountTextBox_TextChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::TextChangedEventArgs^ e)
{
	auto text = dynamic_cast<TextBox^>(sender)->Text;
	auto pos = dynamic_cast<TextBox^>(sender)->SelectionStart;
	std::pair<std::wstring, int> pair = DeleteSymbols(text->Data(), '.', pos);
	pos = pair.second;
	text = ref new String(pair.first.c_str());
	for (int i = 0; i < text->Length(); i++)
	{
		if (!isdigit(text->Data()[i]) && text->Data()[i] != '.')
		{
			auto correct = std::wstring(text->Data());
			correct.erase(i, 1);
			text = ref new String(correct.c_str());
			if (i <= pos) pos--;
		}
	}
	dynamic_cast<TextBox^>(sender)->Text = text;
	dynamic_cast<TextBox^>(sender)->SelectionStart = pos;
}


void Bill::MainPage::SaleTextBox_TextChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::TextChangedEventArgs^ e)
{
	auto text = dynamic_cast<TextBox^>(sender)->Text;
	auto pos = dynamic_cast<TextBox^>(sender)->SelectionStart;
	std::pair<std::wstring,int> pair = DeleteSymbols(text->Data(), '.', pos);
	pos = pair.second;
	text = ref new String(pair.first.c_str());
	for (int i = 0; i < text->Length(); i++)
	{
		if (!isdigit(text->Data()[i])&&i<=1)
		{
			auto correct = std::wstring(text->Data());
			correct.erase(i, 1);
			text = ref new String(correct.c_str());
			if (i <= pos) pos--;
		}
		else if (text->Data()[2] != '.'&&i>=2)
		{
			auto correct = std::wstring(text->Data());
			correct.erase(i, 1);
			text = ref new String(correct.c_str());
			if (i <= pos) pos--;
		}
		else if ((i>4|| ((!isdigit(text->Data()[i])&&i>2)))&& text->Data()[2] == '.')
		{
			auto correct = std::wstring(text->Data());
			correct.erase(i, 1);
			text = ref new String(correct.c_str());
			if (i <= pos) pos--;
		}
	}
	dynamic_cast<TextBox^>(sender)->Text = text;
	dynamic_cast<TextBox^>(sender)->SelectionStart = pos;
}


void Bill::MainPage::CalculateButton_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	if (PriceTextBox->Text->IsEmpty() ||
		CountTextBox->Text->IsEmpty() ||
		SaleTextBox->Text->IsEmpty()) return;
	auto text = PriceTextBox->Text->Data();
	double price = (double)_wtof(text);
	text = CountTextBox->Text->Data();
	double count= (double)_wtof(text);
	text = SaleTextBox->Text->Data();
	double sale= (double)_wtof(text);
	double summ = price*count*(100-sale) / 100;
	SummTextBlock->Text = summ.ToString();
}

std::pair<std::wstring, int> Bill::MainPage::DeleteSymbols(std::wstring str, char s, int pos)
{
	bool first = false;
	for (size_t i = 0;	 i < str.length(); i++)
	{
		if (str[i] == s&&!first) first = true;
		else if (str[i] == s) {
			str.erase(i, 1);
			if (i <= pos) pos--;
		}
	}
	return std::make_pair(str,pos);
}
