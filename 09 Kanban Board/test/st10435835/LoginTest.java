
package st10435835;

/*
    Title: Create and run your first unit test.
    Author: IIEVC School of Computer Science
    Date: 7 March 2022
    Code Version: 1
    Availibility: https://youtu.be/1Pa15vDWG-8?si=HqbbapgGzw9aY9fV
*/

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.assertFalse;
import org.junit.Test;
import org.junit.jupiter.api.BeforeEach;
import org.junit.Test;
import static org.junit.Assert.*;
import static org.junit.jupiter.api.Assertions.assertEquals;
/**
 *
 * @author snikeyi
 */
public class LoginTest {
    
    Login test1 = new Login();
    
 @Test
    public void testCheckUsernameValid() {
        
        assertEquals(true, test1.checkUsername("kyl_1"));
    }

    @Test
    public void testCheckUsernameInvalid() {
        assertEquals(false, test1.checkUsername("kyle!!!!!!!"));
    }
    
    
    @Test
    public void testCheckPasswordComplexityValid() {
        assertEquals(true, test1.checkPasswordComplexity("Ch&&sec@ke99!"));
    }
    
    @Test
    public void testCheckPasswordComplexityInvalid() {
        assertEquals(false, test1.checkUsername("password"));
    }

    
        @Test
    public void testLoginUserInvalid() {
        assertFalse(test1.loginUser("kyle!!!!!!!", "password"));
    }

    @Test
    public void testUsernameValid() {
        assertTrue(test1.checkUsername("kyl_1"));
    }
    
    @Test
    public void testUsernameInvalid() {
        assertFalse(test1.checkUsername("kyle!!!!!!!"));
    }
    
    @Test
    public void testPasswordValid(){
        assertTrue(test1.checkPasswordComplexity("Ch&&sec@ke99!"));
    }
    
        @Test
    public void testPasswordInvalid(){
        assertFalse(test1.checkPasswordComplexity("password"));
    }
    
}
