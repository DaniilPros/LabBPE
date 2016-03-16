//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"
#include <fstream>
#include <iostream>


using namespace TextEdit;
using namespace Windows::Storage::Pickers;
using namespace Windows::Storage;
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
using namespace Windows::UI::Core;
using namespace concurrency;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

MainPage::MainPage()
{
	InitializeComponent(); 
		Phones = ref new Vector<Phone^>();		
		Vector<String^>^ data = ref new Vector<String^>();
		data->Append("Flip");
		data->Append("Bar");
		data->Append("TouchScreen");
		data->Append("Slider");
		FormFactor->ItemsSource = data;
		typeOfNetworks = ref new Vector<String^>();
		typeOfNetworks->Append("CDMA");
		typeOfNetworks->Append("GSM");
		Network->ItemsSource = typeOfNetworks;
		this->ActionButtonToken = (ActionButton->Click += ref new RoutedEventHandler(this, &MainPage::Button_Click));
}


void MainPage::Button_Click(Object^ sender, RoutedEventArgs^ e)
{
	String^ model = ModelTextBox->Text;
	String^ producer = ProducerTextBox->Text;
	auto obj = FormFactor->SelectedValue->ToString();
	String^ formFactor = obj;
	delete obj;
	String^ network = (Network->Text);
	String^ year = (YearTextBox->Text);
	String^ cards = (Cards->Value.ToString());
	String^ price = (PriceTextBox->Text);
	if (model->IsEmpty() ||
		producer->IsEmpty() ||
		formFactor->IsEmpty() ||
		network->IsEmpty() ||
		year->IsEmpty() ||
		cards->IsEmpty() ||
		price->IsEmpty())
	{	
		auto msgDlg = ref new Windows::UI::Popups::MessageDialog("Something can't be empty.\nPlease fill it and try again.", "Something is empty");
		msgDlg->ShowAsync(); 		
	}
	else {
		Phone^ phone = ref new Phone(ModelTextBox->Text,
			ProducerTextBox->Text,
			formFactor,
			Network->Text,
			YearTextBox->Text,
			Cards->Value.ToString(),
			PriceTextBox->Text);
		Phones->Append(phone);
		ModelTextBox->Text = L"";
		ProducerTextBox->Text = L"";
		Network->Text = L"";
		YearTextBox->Text = L"";
		Cards->Value = 0;
		PriceTextBox->Text = L"";
	}

}


void MainPage::Button_PointerEntered(Object^ sender, PointerRoutedEventArgs^ e)
{
	Window::Current->CoreWindow->PointerCursor = ref new CoreCursor(CoreCursorType::Hand, 1);
}


void MainPage::Button_PointerExited(Object^ sender, PointerRoutedEventArgs^ e)
{
	Window::Current->CoreWindow->PointerCursor = ref new CoreCursor(CoreCursorType::Arrow, 1);
}

void TextEdit::MainPage::Grid_RightTapped(Platform::Object^ sender, Windows::UI::Xaml::Input::RightTappedRoutedEventArgs^ e)
{
	auto senderElement = dynamic_cast<FrameworkElement^>(sender);
	FlyoutBase::GetAttachedFlyout(senderElement)->ShowAt(dynamic_cast<FrameworkElement^>(e->OriginalSource));
}


void TextEdit::MainPage::EditButton_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	auto phone = dynamic_cast<Phone^>(dynamic_cast<FrameworkElement^>(e->OriginalSource)->DataContext);
	PhonesView->SelectedValue = phone;
	PhonesView->IsEnabled = false;

	ModelTextBox->Text = phone->model;
	ProducerTextBox->Text = phone->producer;
	YearTextBox->Text = phone->modelYear;
	PriceTextBox->Text = phone->price;
	Network->Text = phone->typeOfNetwork;
	FormFactor->SelectedItem = phone->formFactor;
	Cards->Value = _wtoi(phone->countOfCards->Data());

	ActionButton->Click -= this->ActionButtonToken;
	this->ActionButtonToken = ActionButton->Click += ref new RoutedEventHandler(this, &MainPage::ActionBtnApply_Click);
	ActionButton->Content = L"Apply";
}


void TextEdit::MainPage::DeleteButton_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	auto phone = dynamic_cast<Phone^>(dynamic_cast<FrameworkElement^>(e->OriginalSource)->DataContext);
	auto it = std::find(begin(this->Phones), end(this->Phones), phone);
	auto index = std::distance(begin(this->Phones), it);

	this->Phones->RemoveAt(index);
}

void TextEdit::MainPage::ActionBtnApply_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	String^ model = ModelTextBox->Text;
	String^ producer = ProducerTextBox->Text;
	auto obj = FormFactor->SelectedValue->ToString();
	String^ formFactor = obj;
	delete obj;
	String^ network = (Network->Text);
	String^ year = (YearTextBox->Text);
	String^ cards = (Cards->Value.ToString());
	String^ price = (PriceTextBox->Text);
	if (model->IsEmpty() ||
		producer->IsEmpty() ||
		formFactor->IsEmpty() ||
		network->IsEmpty() ||
		year->IsEmpty() ||
		cards->IsEmpty() ||
		price->IsEmpty())
	{
		auto msgDlg = ref new Windows::UI::Popups::MessageDialog("Something can't be empty.\nPlease fill it and try again.", "Something is empty");
		msgDlg->ShowAsync();
	}
	else {
		auto index = this->PhonesView->SelectedIndex;
		this->Phones->SetAt(index, ref new Phone(ModelTextBox->Text,
			ProducerTextBox->Text,
			formFactor,
			Network->Text,
			YearTextBox->Text,
			Cards->Value.ToString(),
			PriceTextBox->Text));
		PhonesView->IsEnabled = true;

		
		ModelTextBox->Text = L"";
		ProducerTextBox->Text = L"";
		Network->Text = L"";
		YearTextBox->Text = L"";
		Cards->Value = 0;
		PriceTextBox->Text = L"";
		ModelTextBox->Focus(Windows::UI::Xaml::FocusState::Programmatic);

		ActionButton->Click -= this->ActionButtonToken;
		this->ActionButtonToken = ActionButton->Click += ref new RoutedEventHandler(this, &MainPage::Button_Click);
		ActionButton->Content = L"Add";
	}
}

void TextEdit::MainPage::SaveBtn_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	auto picker = ref new FileSavePicker();
	picker->FileTypeChoices->Insert("Plain Text", ref new Vector<String^>{ ".txt" });
	picker->SuggestedFileName = "Collection";
	picker->SuggestedStartLocation = PickerLocationId::Desktop;

	create_task(picker->PickSaveFileAsync()).then([this](StorageFile^ file)
	{
		if (file == nullptr) return;
		auto phones = ref new Vector<String^>();
		for (auto&& phone : this->Phones)
		{
			phones->Append(phone->model);
			phones->Append(phone->producer);
			phones->Append(phone->modelYear);
			phones->Append(phone->formFactor);
			phones->Append(phone->typeOfNetwork);
			phones->Append(phone->countOfCards);
			phones->Append(phone->price);
		}
		CachedFileManager::DeferUpdates(file);
		create_task(FileIO::WriteLinesAsync(file, phones)).then([file]()
		{
			create_task(CachedFileManager::CompleteUpdatesAsync(file)).then([](Windows::Storage::Provider::FileUpdateStatus status)
			{
				Windows::UI::Popups::MessageDialog^ dlg;
				switch (status)
				{
				case Windows::Storage::Provider::FileUpdateStatus::Complete:
					dlg = ref new Windows::UI::Popups::MessageDialog("", "File was saved");
					dlg->ShowAsync();
					break;
				default:
					dlg = ref new Windows::UI::Popups::MessageDialog("", "File couldn't be saved");
					dlg->ShowAsync();
					break;
				}
			});
		});
	});
}


void TextEdit::MainPage::OpenBtn_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	auto picker = ref new FileOpenPicker();
	picker->FileTypeFilter->Append(L".txt");
	picker->SuggestedStartLocation = PickerLocationId::Desktop;
	create_task(picker->PickSingleFileAsync()).then([this](StorageFile^ file)
	{
		if (file == nullptr) return;
		create_task(FileIO::ReadLinesAsync(file)).then([this](IVector<String^>^ lines)
		{
			Phones->Clear();
			for (size_t i = 0; i < lines->Size; i += 7)
			{
				Phones->Append(
					ref new Phone(
						lines->GetAt(i),
						lines->GetAt(i + 1),
						lines->GetAt(i + 2),
						lines->GetAt(i + 3),
						lines->GetAt(i + 4),
						lines->GetAt(i + 5),
						lines->GetAt(i + 6)
						)
					);
			}
		});
	}

	);
}
