
package hospitalpatientregsystem;
import javax.swing.JOptionPane;
public class HospitalPatientRegSystem 
{
    
    public static void main(String[] args) 
    {
        
        int num1 = 3;
        int num2 = 4;
        String name = JOptionPane.showInputDialog(null, "Enter your name:");
        String unName = name.substring(name.length() - 2);
        /*https://stackoverflow.com/questions/8768517/is-there-any-quick-way-to-get-the-last-two-characters-in-a-string*/

        
        String surname = JOptionPane.showInputDialog(null, "Enter your surname:");
        String unSurname = surname.substring(0, Math.min(surname.length(), num1));
        //https://stackoverflow.com/questions/1583940/how-do-i-get-the-first-n-characters-of-a-string-without-checking-the-size-or-goi
        
        String surName = name.concat(" " + surname);
        
        String ID = JOptionPane.showInputDialog(null, "Enter your ID number:");
        String unID = ID.substring(0, Math.min(ID.length(), num2));
        
        String UniqueID = unID + unName + unSurname;
        
        JOptionPane.showMessageDialog(null, "*_********Hospital Patient Record********_*\n"
                + "Welcome to Greys Hospital " + surName+ "\n"
                        + "Your unique ID will be: " + UniqueID.toLowerCase());
        
        
    }
    
}
