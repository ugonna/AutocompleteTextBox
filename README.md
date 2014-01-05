AutocompleteTextBox
===================

A TextBox for WinRT apps that uses a custom dictionary of suggestions to popup autocomplete options.

Why?
----

I needed a simple textbox with custom autocomplete suggestions for my app, [Eduplanner](http://goo.gl/pTODw "Eduplanner WinRT app"), and I couldn't find any on the internet (surprising, I know) so I built a simple one.

Do with it as you please.

Usage?
------

The project is a Windows Runtime Component so it can be used for any WinRT project. To import and use in your page with XAML:

```xml

<Page ...
      xmlns:actb="using:AutocompleteTextBox"
      ...
      >

...

<actb:AutocompleteTextBox x:Name = "countryTextBox" FontSize="20"
                          PlaceholderText="Type in country names"
                          ItemsSource="{Binding}"/>
...

```

Use it in code as you would any normal ComboBox. C# sample:

```csharp

List<string> countries = new List<string>();
countries.Add("Nigeria");
countries.Add("United States");
countries.Add("United Kingdom");

countryTextBox.DataContext = countries;

```



