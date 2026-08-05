package st10435835;

import org.junit.Test;
import static org.junit.Assert.*;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import org.junit.Test;
import org.junit.jupiter.api.BeforeEach;
import static org.junit.jupiter.api.Assertions.assertEquals;
import org.junit.Test;
import st10435835.Task;

public class TaskTest {

    Task test1 = new Task();


    @Test
    public void testTaskDescriptionValid() 
    {
        assertEquals(true, test1.checkTaskDescription("Short description"));// Valid description
    }
    
    @Test
    public void testTaskDescriptionInvalid() 
    {
        assertEquals(false, test1.checkTaskDescription("This description is way too long and definitely exceeds fifty characters."));// Invalid description
    }

    @Test
    public void testCreateTaskID() 
    {
        String taskID = test1.createTaskID("Login Feature", 0, "Robyn");
        assertEquals("LO:0:BYN", taskID); // Example output based on developer name and task name from Test Data for Task 1
    }



    @Test
    public void testTotalHoursAccumalated() 
    {
        test1.taskDur = 8;
        test1.returnTotalHours();
        test1.taskDur = 10;
        float totalHours = test1.returnTotalHours();
        assertEquals(18, totalHours); // Example output based on total hours from Test Data for Task 1 and 2
    }
    
    
    Task Test2 = new Task();
    
    @Test
    public void testToReturnDevDetails() 
    {
        
        Test2.addEntry("Create Login", " ", 0, 5, 0, " ", "To Do", 1, "Mike", "Smith");
        Test2.addEntry("Create Add Features", " ", 1, 8, 0, " ", "Doing", 3, "Edward", "Harrison");
        Test2.addEntry("Create Reports", " ", 2, 2, 0, " ", "Done", 2, "Samantha", "Paulson");
        Test2.addEntry("Add Arrays", " ", 3, 11, 0, " ", "To Do", 1, "Glenda", "Oberholzer");
        
        assertEquals("Mike", Task.arrDevNames[0]);
        assertEquals("Smith", Task.arrDevSurnames[0]);
        
        assertEquals("Edward", Task.arrDevNames[1]);
        assertEquals("Harrison", Task.arrDevSurnames[1]);
        
        assertEquals("Samantha", Task.arrDevNames[2]);
        assertEquals("Paulson", Task.arrDevSurnames[2]);
        
        assertEquals("Glenda", Task.arrDevNames[3]);
        assertEquals("Oberholzer", Task.arrDevSurnames[3]);
    }
    
    @Test
    public void testLongestDuration()
    {
        
        Test2.addEntry("Create Login", " ", 0, 5, 0, " ", "To Do", 1, "Mike", "Smith");
        Test2.addEntry("Create Add Features", " ", 1, 8, 0, " ", "Doing", 3, "Edward", "Harrison");
        Test2.addEntry("Create Reports", " ", 2, 2, 0, " ", "Done", 2, "Samantha", "Paulson");
        Test2.addEntry("Add Arrays", " ", 3, 11, 0, " ", "To Do", 1, "Glenda", "Oberholzer");
        
        Test2.findLongestDuration();
        assertEquals("Glenda", Task.arrDevNames[3]);
        assertEquals("Oberholzer", Task.arrDevSurnames[3]);
        assertEquals(11, Task.arrDurations[3]);
        
    }
    
    @Test
    public void searchForTasks()
    {
        
        Test2.addEntry("Create Login", " ", 0, 5, 0, " ", "To Do", 1, "Mike", "Smith");
        Test2.addEntry("Create Add Features", " ", 1, 8, 0, " ", "Doing", 3, "Edward", "Harrison");
        Test2.addEntry("Create Reports", " ", 2, 2, 0, " ", "Done", 2, "Samantha", "Paulson");
        Test2.addEntry("Add Arrays", " ", 3, 11, 0, " ", "To Do", 1, "Glenda", "Oberholzer");
        
        Test2.SearchTasksByName("Create Login");
        assertEquals("Mike", Task.arrDevNames[0]);
        assertEquals("Smith", Task.arrDevSurnames[0]);
        assertEquals("Create Login", Task.arrTaskName[0]);
        
    }    
    
    @Test
    public void searchTasksAssignedToDeveloper()
    {
        
        Test2.addEntry("Create Login", " ", 0, 5, 0, " ", "To Do", 1, "Mike", "Smith");
        Test2.addEntry("Create Add Features", " ", 1, 8, 0, " ", "Doing", 3, "Edward", "Harrison");
        Test2.addEntry("Create Reports", " ", 2, 2, 0, " ", "Done", 2, "Samantha", "Paulson");
        Test2.addEntry("Add Arrays", " ", 3, 11, 0, " ", "To Do", 1, "Glenda", "Oberholzer");
        
        Test2.SearchTasksByDevname("Samantha");
        assertEquals("Samantha", Task.arrDevNames[2]);
        assertEquals("Paulson", Task.arrDevSurnames[2]);
        assertEquals("Create Reports", Task.arrTaskName[2]);
        
    } 
    
    
    @Test
    public void deleteTask()
    {
        
        Test2.addEntry("Create Login", " ", 0, 5, 0, " ", "To Do", 1, "Mike", "Smith");
        Test2.addEntry("Create Add Features", " ", 1, 8, 0, " ", "Doing", 3, "Edward", "Harrison");
        Test2.addEntry("Create Reports", " ", 2, 2, 0, " ", "Done", 2, "Samantha", "Paulson");
        Test2.addEntry("Add Arrays", " ", 3, 11, 0, " ", "To Do", 1, "Glenda", "Oberholzer");
        
        Test2.RemoveTasks("Create Reports");
        
    } 
    
    /*public void checkDevDetails() 
    {
        Test2.AddTasks("Create Login", "Mike", "Smith", 5, 1, " ");
        
            for(int i=0; i<4; i++)
            {
                
            }
       
    }
    
        
    public String TestForReturningDevDetails()
    {
       
    for (int i = 0; i < arrTaskName.length; i++) {
        if (arrTaskName[i] != null) 
        {
             devDetails = arrDevNames[i].concat(" " + arrDevSurnames[i]);    
        }
    }
    return devDetails;
    }*/

}

