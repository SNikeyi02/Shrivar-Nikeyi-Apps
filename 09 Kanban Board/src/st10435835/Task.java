
package st10435835;

import javax.swing.JOptionPane;

public class Task {
    public String taskName;
    public String taskDescription;
    public String unTaskID;
    public int taskNum;
    public int taskDur;
    public int totalHours = 0;
    public String taskStatus;
    public String devName;
    public String devSurname;
    public int taskStat;
    public int numOfTasks;

    // create arrays to hold new entries. Set to hold a maximum of 20 values. 
    static String[] arrTaskName = new String[20]; // contains the names of all the Tasks
    static String[] arrDevNames = new String[20]; // contains the names of all the developers
    static String[] arrDevSurnames = new String[20]; // contains the surnames of all the developers
    static int[] arrDurations = new int[20]; // contains the duration of all tasks
    static int[] arrTaskStatus = new int[20]; // contains the status of all tasks
    static String[] arrUnTaskID = new String[20]; // contains the generated transaction ID's for all tasks

    public void addEntry(String taskName, String unTaskID, int taskNum, int taskDuration, int totalHours, String taskDescription, String taskStatus, int taskStat, String devName, String devSurname) {
        this.taskName = taskName;//addEntry is created as it creates an object of all the data for testing
        this.unTaskID = unTaskID;
        this.taskNum = taskNum;
        taskNum++;
        this.taskDur = taskDuration;
        this.totalHours = totalHours;
        checkTaskDescription(taskDescription);
        this.taskDescription = taskDescription;
        this.taskStatus = taskStatus;
        this.taskStat = taskStat;
        this.devName = devName;
        this.devSurname = devSurname;
        createTaskID(taskName, taskNum, devName);
        populateArray();  // Ensure arrays are populated
        printTaskDetails();
    }

    public Task() {}

    public Task(int taskNum, String taskName, String devName, String devSurname, int taskDur, int taskStat)//created this to populate arrayas 
    {
        this.taskNum = taskNum;
        this.taskName = taskName;
        this.devName = devName;
        this.devSurname = devSurname;
        this.taskStat = taskStat;
        this.taskDur = taskDur;
        populateArray();
        
    }

    public boolean checkTaskDescription(String taskDescription) 
    {
        return taskDescription.length() < 50;//creates a control to not make task description entered exceed 50 letters entered
    }

    public String createTaskID(String taskName, int taskNum, String devName) {
        String unTaskName = taskName.substring(0, Math.min(taskName.length(), 2));//extracts first 2 letters from the task name and assigns it to unTaskName
        String unName = devName.substring(devName.length() - 3);//extracts last 3 letters of devName and assigns it to unName
        unTaskID = (unTaskName + ":" + taskNum + ":" + unName).toUpperCase();//creates a single variable that holds the unique task ID in it called unTaskID
        return unTaskID;//returns it to the main method
    }

    private String TaskStatus(int taskStat) {
        switch (taskStat) {// a mtheod that houses the switch cases for the task statuses
            case 1:
                return "To Do";
            case 2:
                return "Done";
            case 3:
                return "Doing";
            default:
                taskStat = Integer.parseInt(JOptionPane.showInputDialog("Please enter the status of the task:\n1.To Do \n2.Done \n3.Doing "));// prompts user for their entry
                return TaskStatus(taskStat);// returns the number the user enters to the method it will be used in 
        }
    }

    private String printTaskDetails() {
        StringBuilder display = new StringBuilder();// prints the details of the tasks as they get added
        display.append("\nTask Status: ").append(TaskStatus(taskStat));
        display.append("\nDeveloper Details: ").append(devName).append(" ").append(devSurname);
        display.append("\nTask Number: ").append(taskNum);// makes use of a string builder
        display.append("\nTask Name: ").append(taskName);
        display.append("\nTask Description: ").append(taskDescription);
        display.append("\nTask ID: ").append(unTaskID);
        display.append("\nTask Duration: ").append(taskDur).append(" hours");
        display.append("\nTotal Hours taken for all tasks: ").append(returnTotalHours()).append(" hours");
        return display.toString();// returns the display to the string builder
    }

    public float returnTotalHours() {
        totalHours += taskDur;// calculation that adds each tasks duration
        return totalHours;//accumaled in totalHours variable and is returned
    }

    public void options() {
        String[] Options = { "Add tasks", "Show report", "Quit" };// makes use of an array that shows options as boxes
        int choice = JOptionPane.showOptionDialog(
            null,
            "Choose an option:",//prompts user to choose an option
            "HeisenBugs",// name of program
            JOptionPane.DEFAULT_OPTION,
            JOptionPane.QUESTION_MESSAGE,
            null,
            Options,
            Options[0] 
        );

        while (!(choice == 2)) {//creates a loop that is continous unless user selects option 2, which is to quit
            switch (choice) {// uses switch statments to house the above options actions
                case 0:
                    numOfTasks = Integer.parseInt(JOptionPane.showInputDialog(null, "Enter the number of tasks you wish to add:"));// ask user for how many tasks they want to add
                    taskNum = 0;// sets the counter to 0
                    AddTasks(taskName, devName, devSurname, taskDur, taskStat, taskDescription);// calls the method to add the tasks
                    break;// break is added as it will allow to end after this case and not automatically go to the next
                
                case 1:
                    ShowReport();//calls the method that shows report after you add tasks
                    break;
                
                case 2:
                    System.exit(0);//if user chooses this, it would end the program
                    break;
                
                default:
                    JOptionPane.showMessageDialog(null, "Invalid Option");// a default message for when an invalid input is given
                    break;  
            }
                  
            choice = JOptionPane.showOptionDialog(//shows the option dialogs again as it is in a loop
                null,
                "Select your task:",
                "HeisenBugs",
                JOptionPane.DEFAULT_OPTION,
                JOptionPane.QUESTION_MESSAGE,
                null,
                Options,
                Options[0] 
            );    
        }
    }

    // Getter methods for testing
    public String getTaskName() { return taskName; }
    public String getTaskDescription() { return taskDescription; }
    public String getUnTaskID() { return unTaskID; }
    public int getTaskNum() { return taskNum; }
    public float getTaskDur() { return taskDur; }
    public float getTotalHours() { return totalHours; }
    public String getTaskStatus() { return taskStatus; }
    public String getDevName() { return devName; }
    public String getDevSurname() { return devSurname; }
    public int getTaskStat() { return taskStat; }
    
    public void AddTasks(String taskName, String devName, String devSurname, int taskDur, int taskStat, String taskDescription)// will add a task to the arrays
    {
                    while(numOfTasks > 0) //loop is created staing it must be greater than 0 as the counter decrements
                    {
                        taskName = JOptionPane.showInputDialog(null, "Enter the name of the task:");//prompts users for entrys
                        devName = JOptionPane.showInputDialog(null, "Enter the first name of the Developer assigned to the task:");
                        devSurname = JOptionPane.showInputDialog(null, "Enter the surname of the Developer assigned to the task:");
                        taskDur = Integer.parseInt(JOptionPane.showInputDialog(null, "Enter the estimated duration of the task (in hours):"));
                        taskStat = Integer.parseInt(JOptionPane.showInputDialog("Please enter the status of the task:\n1.To Do \n2.Done \n3.Doing "));
                        taskDescription = JOptionPane.showInputDialog(null, "Enter a short description of the task:");
                        
                        if(checkTaskDescription(taskDescription)) {//calls method to check whether task description is valid
                            JOptionPane.showMessageDialog(null, "Task successfully captured");
                        } else {
                            JOptionPane.showMessageDialog(null, "Please enter a task description of less than 50 characters");
                        }
                        
                        addEntry(taskName, unTaskID, taskNum, taskDur, totalHours, taskDescription, taskStatus, taskStat, devName, devSurname);
                        JOptionPane.showMessageDialog(null, printTaskDetails());//after all values are entered, it would show a summary of what was entered
                        
                        taskNum++;//incrementsthe task number to add additional tasks
                        numOfTasks--;//decrements the number of tasks to enter as you have just entered one
                    }
    }
    
    public void ShowReport()// the second option in the Options method. it is used to display all options availible to manipulate the arrays
    {
        String choice2 = JOptionPane.showInputDialog("Please select an option:\n1-Display Tasks that are Done\n2-Find the task with the longest duration"
                + "\n3-Search for Task by name\n4-Search for tasks assigned to developers\n5-Delete a task by entering its name\n6-Display a full report.");
                    int option = Integer.parseInt(choice2);//asks user for input on their choice
                    
                    switch (option) {
                        case 1:
                            displayDoneTasks();//will display all the tasks that are marked done
                            break;
                        case 2:
                            findLongestDuration();//will find the task that has the longest duration entered
                            break;
                        case 3:
                            String taskSearchValue = JOptionPane.showInputDialog("Enter task name to search by");
                            SearchTasksByName(taskSearchValue);//will search for a task when you enter its name 
                            break;
                        case 4:
                            String devNameSearchValue = JOptionPane.showInputDialog("Enter developer name to search by");
                            SearchTasksByDevname(devNameSearchValue);//will search for the task assigned to the developer by entering the developers name 
                            break;
                        case 5:
                            String deleteValue = JOptionPane.showInputDialog("Enter task name to search by");
                            RemoveTasks(deleteValue);//when you enter a task name that was entered, it would delete the task from your entries
                            break;
                        case 6:
                            displayFullReportArray();
                            break;//will display all the tasks that where entered
                        default:
                            throw new AssertionError();//when an invalid input is entered
                    }
    }

    
    // create method to populate arrays with transaction details
    private void populateArray() {
        int position = taskNum; // position of transaction in the array is equal to the transaction number 
        arrDevNames[position] = devName; // add recipient of transaction to array 
        arrDevSurnames[position] = devSurname; // add name of transaction to array 
        arrTaskName[position] = taskName; // add id of transaction to array 
        arrDurations[position] = taskDur; // add amount of transaction to array 
        arrTaskStatus[position] = taskStat; // add status of transaction to array 
        arrUnTaskID[position] = unTaskID; // add description of transaction to array 
    }

    private void displayFullReportArray() {//option 6 in the showReport method
    StringBuilder report = new StringBuilder();//use a string builder
    for (int i = 0; i < arrTaskName.length; i++) {//create a for loop
        if (arrTaskName[i] != null) {//if the task name in array position i has nothing in it, going to return an empty string
            String status = "";
            if (arrTaskStatus[i] == 1) {//creates another declaration that confirms the task status
                status = "To Do";
            } else if (arrTaskStatus[i] == 2) {
                status = "Done";
            } else if (arrTaskStatus[i] == 3) {
                status = "Doing";
            }
            report.append("\nTask ").append(i);// appends the task number to this
            
            report.append("\nDeveloper: ").append(arrDevNames[i]).append(" ").append(arrDevSurnames[i]);//returns these details
            report.append("\nTask Name: ").append(arrTaskName[i]);
            report.append("\nTask Duration: ").append(arrDurations[i]);
            report.append("\nTask Status: ").append(status);
            report.append("\n");
        }
    }
    if (report.length() == 0) {// creates a condition where if no task are found or has been entered
        JOptionPane.showMessageDialog(null, "No tasks found.");//>it would return this string
    } else {
        JOptionPane.showMessageDialog(null, report.toString());//is it does find tasks entered, it will pull the string builder to display the details
    }
}

    private void displayDoneTasks() {// method to display all the tasks that were marked as done
        for (int i = 0; i < arrTaskName.length; i++) {
            if (arrTaskStatus[i] == 2) {// since done in the case statement is option 2
                StringBuilder display = new StringBuilder();//>it would display all tasks that the user chose option 2
                display.append("\nTask Status: Done");
                display.append("\nDeveloper Details: ").append(arrDevNames[i]).append(" ").append(arrDevSurnames[i]);
                display.append("\nTask Name: ").append(arrTaskName[i]);
                display.append("\nTask Duration: ").append(arrDurations[i]).append(" hours");
                JOptionPane.showMessageDialog(null, display.toString());
            }
        }
    }

    public void findLongestDuration() {//method to display the task with the longest duration
        int longestIndex = 0;//create a local variable and set it to 0
        for (int i = 1; i < arrDurations.length; i++) {//create a for loop
            if (arrDurations[i] > arrDurations[longestIndex]) {//check every array duration entered for the largest value
                longestIndex = i;//the largest value will be occupied by longestIndex variable
            }
        }
        StringBuilder display = new StringBuilder();//use string builder to display details of the task with the longest duration
        display.append("\nTask with the longest duration:");
        display.append("\nDeveloper Details: ").append(arrDevNames[longestIndex]).append(" ").append(arrDevSurnames[longestIndex]);
        display.append("\nTask Duration: ").append(arrDurations[longestIndex]).append(" hours");
        JOptionPane.showMessageDialog(null, display.toString());
    }

    public void SearchTasksByName(String taskSearchValue) //method used to search for tasks by entering its name
    {
    boolean found = false;//use a boolean expression and set it to false as it hasn't been found
    for (int i = 0; i < arrTaskName.length; i++) {//use a for loop
        if (arrTaskName[i] != null && arrTaskName[i].equalsIgnoreCase(taskSearchValue)) {//checks whether the task name entered in the addTask method matches the search phrase used to search for the task name
            String status = "";
            if (arrTaskStatus[i] == 1) {
                status = "To Do";
            } else if (arrTaskStatus[i] == 2) {
                status = "Done";
            } else if (arrTaskStatus[i] == 3) {
                status = "Doing";
            }
            StringBuilder display = new StringBuilder();
            display.append("\nTask Name: ").append(arrTaskName[i]);
            display.append("\nDeveloper assigned to Task: ").append(arrDevNames[i]).append(" " + arrDevSurnames[i]);
            display.append("\nTask Status: ").append(status);
            JOptionPane.showMessageDialog(null, display.toString());
            found = true;//if it is found, the boolean becomes true
            break;
        }
    }
    if (!found) {//if the string entered does not match any task name it would display
        JOptionPane.showMessageDialog(null, "Task name not found.");//>display this string
    }
}

    public void SearchTasksByDevname(String devNameSearchValue) {//method used to search for tasks by entering the developers name assigned to the task
        boolean found = false;//use a boolean expression and set it to false as it hasn't been found
    for (int i = 0; i < arrTaskName.length; i++) {//use a for loop
        if (arrDevNames[i] != null && arrDevNames[i].equalsIgnoreCase(devNameSearchValue)) {//checks whether the developers name entered in the addTask method matches the search phrase used to search for the developers name
            String status = "";
            if (arrTaskStatus[i] == 1) {
                status = "To Do";
            } else if (arrTaskStatus[i] == 2) {
                status = "Done";
            } else if (arrTaskStatus[i] == 3) {
                status = "Doing";
            }
            StringBuilder display = new StringBuilder();
            display.append("\nTask Name: ").append(arrTaskName[i]);
            display.append("\nTask Status: ").append(status);
            JOptionPane.showMessageDialog(null, display.toString());
            found = true;//if it is found, the boolean becomes true
            break;
        }
    }
    if (!found) {//if the string entered does not match any developers name it would display
        JOptionPane.showMessageDialog(null, "Developer name not found.");//>display this string
    }
    }

    public void RemoveTasks(String taskNameToRemove) {//method to delete a task by entering the task name
    boolean found = false;//use a boolean expression and set it to false as it hasn't been found
    for (int i = 0; i < arrTaskName.length; i++) {//use a for loop
        if (arrTaskName[i] != null && arrTaskName[i].equalsIgnoreCase(taskNameToRemove)) {//checks whether the task name entered in the addTask method matches the search phrase used to search for the task name
            // Shift all elements one position to the left
            for (int j = i; j < arrTaskName.length - 1; j++) {//instead of deleting the task, i just remove it from the array
                arrTaskName[j] = arrTaskName[j + 1];
                arrDevNames[j] = arrDevNames[j + 1];
                arrDevSurnames[j] = arrDevSurnames[j + 1];
                arrDurations[j] = arrDurations[j + 1];
                arrTaskStatus[j] = arrTaskStatus[j + 1];
                arrUnTaskID[j] = arrUnTaskID[j + 1];
            }
            // Set the last element to null/empty since it's shifted left
            arrTaskName[arrTaskName.length - 1] = null;
            arrDevNames[arrDevNames.length - 1] = null;
            arrDevSurnames[arrDevSurnames.length - 1] = null;
            arrDurations[arrDurations.length - 1] = 0;
            arrTaskStatus[arrTaskStatus.length - 1] = 0;
            arrUnTaskID[arrUnTaskID.length - 1] = null;
            
            JOptionPane.showMessageDialog(null, "Task removed successfully.");
            found = true;
            break;
        }
    }
    if (!found) {//if the string entered does not match any task name it would display
        JOptionPane.showMessageDialog(null, "Task name not found.");//>display this string
    }
}

}
