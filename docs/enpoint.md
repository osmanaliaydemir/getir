🔐 AUTH & USER

POST /api/auth/register

POST /api/auth/login

POST /api/auth/refresh

POST /api/auth/logout

POST /api/auth/forgotpassword

POST /api/auth/resetpassword

POST /api/auth/sendverification

GET /api/users/getprofile

PUT /api/users/updateprofile

GET /api/users/getaddresses

POST /api/users/addaddress

PUT /api/users/updateaddress/{id}

DELETE /api/users/deleteaddress/{id}

PUT /api/users/setdefaultaddress/{id}

🏪 MERCHANTS (Market & Restoran)

GET /api/merchants/listmerchants

GET /api/merchants/getmerchant/{id}

POST /api/merchants/createmerchant

PUT /api/merchants/updatemerchant/{id}

DELETE /api/merchants/deletemerchant/{id}

PUT /api/merchants/setworkinghours/{id}

PUT /api/merchants/setdeliveryareas/{id}

PUT /api/merchants/togglestatus/{id}

GET /api/merchants/listnearby

📂 CATEGORIES

GET /api/categories/listcategories

GET /api/categories/getcategory/{id}

POST /api/categories/createcategory

PUT /api/categories/updatecategory/{id}

DELETE /api/categories/deletecategory/{id}

🍔 PRODUCTS

GET /api/products/listbymerchant/{merchantId}

GET /api/products/getproduct/{id}

POST /api/products/createproduct

PUT /api/products/updateproduct/{id}

DELETE /api/products/deleteproduct/{id}

POST /api/products/updatestock/{id}

POST /api/products/addoption/{id}

DELETE /api/products/deleteoption/{id}

🛒 CART

GET /api/cart/getcart

POST /api/cart/additem

PUT /api/cart/updateitem/{id}

DELETE /api/cart/deleteitem/{id}

DELETE /api/cart/clearcart

POST /api/cart/applycoupon

DELETE /api/cart/removecoupon

📦 ORDERS (Müşteri)

POST /api/orders/createorder

GET /api/orders/getorder/{id}

GET /api/orders/listorders

PUT /api/orders/cancelorder/{id}

POST /api/orders/reorder/{id}

GET /api/orders/trackorder/{id}

📦 ORDERS (Merchant Paneli)

GET /api/merchant/orders/listincoming

GET /api/merchant/orders/getorderdetail/{id}

PUT /api/merchant/orders/acceptorder/{id}

PUT /api/merchant/orders/rejectorder/{id}

PUT /api/merchant/orders/setpreparing/{id}

PUT /api/merchant/orders/setready/{id}

PUT /api/merchant/orders/setontheway/{id}

PUT /api/merchant/orders/setdelivered/{id}

🚴 COURIER

GET /api/courier/orders/listassigned

PUT /api/courier/orders/pickup/{id}

PUT /api/courier/orders/setontheway/{id}

PUT /api/courier/orders/setdelivered/{id}

POST /api/courier/location/update

POST /api/courier/availability/set

💳 PAYMENT

POST /api/payment/checkout

POST /api/payment/callback

GET /api/payment/getstatus/{orderId}

POST /api/payment/savecard

DELETE /api/payment/deletecard/{id}

🎁 CAMPAIGNS & LOYALTY

GET /api/campaigns/listcampaigns

POST /api/coupons/validatecoupon

GET /api/loyalty/getpoints

POST /api/loyalty/redeempoints

🔔 NOTIFICATIONS

GET /api/notifications/listnotifications

POST /api/notifications/markasread

POST /api/notifications/subscribe

POST /api/notifications/unsubscribe

🔎 SEARCH

GET /api/search/searchproducts

GET /api/search/searchmerchants