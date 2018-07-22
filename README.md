# nopCommerce "Has products" discount requirement plugin
With this plugin, you can create discount requirements that checks the total quantity of selected products against a predefined range. For example, you can select 10 different products (let's say Product A, B, C, ...) and set minimum and maximum quantities to 3 and 5 respectively. In this case, discount will be applied if customer's cart contains any of these products with a total quantity of 3, 4 or 5, so to get this discount a customer can buy

* 3 pieces of Product A or
* 2 pieces of Product B and 1 piece of Product C

By this way, it's really easy to create discounts like

* Buy X of these products and get Y free
* Buy X of these products and get Y% discount

In addition to the discount requirement, you will get a user friendly product selection window. While selecting products, you don't need to leave selection window anymore. All of your selections will be kept even if you perform a search or go to another page by using pagination. There is also a summary list that you can see all your selected products, so that you can quickly deselect unwanted products.

### Installation
* Download [latest release](https://github.com/nopWay/HasProducts-discount-requiremement-plugin-for-nopcommerce/releases/latest) and copy "DiscountRules.HasProducts" folder to Presentation/Nop.Web/Plugins directory.
* Go to yourdomain.com/Admin/Plugin/List and click "Reload List of Plugins" button
* Scroll down to "Cart must contain certain amount of products" plugin and click "Install" button

### Usage
* After creating a discount open "Reqirements" tab
* Select "Cart must contain a certain amount of products" as discount requirement type
* Set minimum and maximum quantities. The total quantity of selected products appearing in the cart should be between min and max values in order to this discount to be applied.
* Select products. This field accepts a comma-separated list of valid product ids. Please note that you can't set quantity or range values after product ids.
* Save requirement

### Attributions
* Discount tag icon in the logo by Vectors Market from the Noun Project

### License
MIT Copyright (c) 2018 nopWay