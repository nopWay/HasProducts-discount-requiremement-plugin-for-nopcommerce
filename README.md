# nopCommerce "Has products" discount requirement plugin
This nopCommerce plugin allows you to configure discounts for customers who have a certain amount of products in the cart

### Installation
* Download latest release and copy "Nop.Plugin.Discount.HasProducts" folder to Nop.Web/Plugins directory.
* Go to yourdomain.com/Admin/Plugin/List and click "Reload List of Plugins" button
* Scroll down to "Cart must contain a certain amount of products" plugin and click "Install" button

### Usage
* After creating a discount open "Reqirements" tab
* Select "Cart must contain a certain amount of products" as discount requirement type
* Set minimum and maximum quantities. The total quantity of selected products appearing in the cart should be between min and max values in order to this discount to be applied.
* Select products that their total quantity will be checked against min and max values. This field accepts a comma-separated list of valid product ids. Please note that you can't set quantity or range values after product ids.
* Save requirement

### Attributions
* Discount tag icon in the logo by Vectors Market from the Noun Project

### License
MIT Copyright (c) 2018 nopWay