# description of changes required

All of the current Angular 20 code is located at src

I would like to implement a new component that manages the current status of the logged in user by:

1. displaying the username and and to the right a link that displays 'Logout'. When this link is clicked, it will logout the current user. The back end endpoint is make up of a baseApiUrl (stored in the environment.ts) and the '/user/logout'. When the user logs out, we need the user keey in the local storage to be set to null.

2. When there is no current user logged in, this component will have a link that display 'Login'. When this is clicked, a popup login modal is displayed - this will be a login component. Below is the features of the login component:

2.1. The modal will have a username/email text box where the user can enter in the user name or email addresss. There will also be a password textbox where the password can be entered.

2.2. The modal will have a button that says 'Login' where if clicked, the login details are sent to a user-service.service.ts where the backend calls are kept to login the user. The back end endpoint is make up of a baseApiUrl (stored in the environment.ts) and the '/user/authenticate'. This is a post and the body of the post must include the following json object:

        {
        "username": "admin",
        "password": "AppleRocks!"
        }

        Note: the username 'admin' and password 'AppleRocks!' will come from the user login modal popup.

2.3. If the login is suscessful, we need to store the user object returned from /authenticate into local storage under a key 'user'. If the login is unsuccessful, then an error message will be displayed indicating that the login has failed - but keep the message ambigious - not telling the user what is wrong.

2.4. If the login is successful, then we will close the modal popup and display the current username in the user status component. There will also be a cancel button on the modal login popup where when clicked, no user is logged in and the current user is empty and the modal popup will close.



