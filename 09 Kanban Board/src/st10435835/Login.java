
package st10435835;

import javax.swing.JOptionPane;




public class Login 
{
    
    private String username;//declare variables as private as they are not to be acessed by other users 
    private String password;
    private String name;
    private String surname;
    
    
// method that 
public void addUser(String username, String password, String name, String surname)
{
    this.name = name;
    this.surname = surname;
    this.username = username;
    this.password = password;
}

// method that will check whether 
public boolean checkUsername(String username)
    {
        if(username.length()<=5 && username.contains("_"))// set parameters for the username according to POE using if statements and logical connectives
        {
            return true;// return true if requirments are met
        }
        else
        {
            return false;// return false if the username does not meet parameters set
        }
    }

public boolean checkPasswordComplexity(String password)
    {
        String regex = "^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#&()–[{}]:;',?/*~$^+=<>]).{8,20}$";
        return password.matches(regex);// using a regex to check if password is correctly formatted
    }
    
    /*
    Title: Java regex validate password examples
    Author: Mkyong
    Date: 5 November 2020
    Code Version: 1
    Availibility: https://mkyong.com/regular-expressions/how-to-validate-password-with-regular-expression/
    */

    
public String registerUser()
    {
        if(!checkUsername(username))// use if statements to display messages when username and/or password does or doesnt meet requirements
        {
            JOptionPane.showMessageDialog(null, "Username is not correctly formatted, "
                    + "\nplease ensure that your username contains an underscore "
                    + "\nand is no more than 5 characters in length");// will display if username is not in the correct format
            return username;
        }
        else if (!checkPasswordComplexity(password))
        {
            JOptionPane.showMessageDialog(null, "Password is not correctly formatted, "
                    + "\nplease ensure that the password contains at least 8 characters, "
                    + "\na capital letter, a number and a special character.");// will display if password is not in the correct format
            return password;
        }
        else
            JOptionPane.showMessageDialog(null, "Username and Password successfully captured");
            return username;// when the username and password both meet the parameters
    }

public boolean loginUser (String enteredUsername, String enteredPassword)
    {
        if (enteredUsername.equals(username) && enteredPassword.equals(password))// checks whether the username and password that the user uses to login in matches with what they entered to register
        {
            return true;
        }
        else
        {
            return false;// if it doesnt match it will return false
        }
    }
    

public void loginDetails()
{
        name = JOptionPane.showInputDialog("Please enter your name: ");//user input for name and surname
        surname = JOptionPane.showInputDialog("Please enter your surname: ");
        
        boolean registered = false;// create a boolean (true or false) variable to hold the login state of the user 
        
        while (!registered)
        {
            username = JOptionPane.showInputDialog(null, "Please create a username: ");// user sets a username and password
            password = JOptionPane.showInputDialog(null, "Please create a password: ");
            
            if (checkPasswordComplexity(password) && checkUsername(username))
            {
                JOptionPane.showMessageDialog(null, "Thank you, you will be taken to login \nwith your recently created credentials");

                // successful login >> loggedIn = true so the application will no longer ask to login
                registered = true;
            }
            
            // Password or accountID incorrect >> loggedIn still false >> ask the user to try again
            else 
            { 
                registerUser();
            }
            
        }// end while loop >> loop will run until loggedIn = true >> until password and accountID are correct
        
        
        boolean loggedIn = false;// create a boolean (true or false) variable to hold the login state of the user 
        //>> if user login is successful then loggedIn is true >> else loggedIn is false >> user cant access features
        
        while (!loggedIn) { // create a while loop
        //>> this will allow the user to keep trying to login while loggedIn is false >> !(not) true
        // >> once the user is logged in the while loop will end >> loggedIn = true
        
            String enteredUsername = JOptionPane.showInputDialog(null, "Please enter your username to login: ");//user enters the set username and password that they have entered above
            String enteredPassword = JOptionPane.showInputDialog(null, "Please enter your password to login: ");// ask user for accountID and password using JOption pane

            String surName = name.concat(" " + surname);

            // call the checkPasswordComplexity method to check that the password is correct
            // call the checkUsername method to check that the accountID is correct
            // if both are correct (&&) then the user can login and access features
            if (loginUser(enteredUsername, enteredPassword))
            {
                JOptionPane.showMessageDialog(null, "Welcome " + surName + " to HeisenBugs");
                // call the options method to display all the features to the user
                
                // successful login >> loggedIn = true so the application will no longer ask to login
                loggedIn = true;
                
            }
            
            // Password or accountID incorrect >> loggedIn still false >> ask the user to try again
            else
            { 
                JOptionPane.showMessageDialog(null, "Invalid details please try again");
            }
            
        
        }
}
}

        




    

