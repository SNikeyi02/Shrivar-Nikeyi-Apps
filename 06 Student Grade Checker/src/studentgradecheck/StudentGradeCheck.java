
package studentgradecheck;
import java.util.Scanner;
import javax.swing.JOptionPane;
public class StudentGradeCheck 
{
    
      public static void gradeCalculation(int num)
        {
           if (num >= 75 && num <= 100)
                    {
                       JOptionPane.showMessageDialog(null, "You Passed with a distinction!");
                   }
           else if (num >= 50 && num <= 74)
           {
               JOptionPane.showMessageDialog(null, "You have passed!");
           }
    
        }
    public static void main(String[] args) 
    {
        
        String inum = JOptionPane.showInputDialog(null, "Please enter your grade: ");
        int num = Integer.parseInt(inum);
        gradeCalculation(num);
        //Scanner scanner = new Scanner(System.in);
        //System.out.println("Please Enter Your Exam Score:");
        //int examScore = scanner.nextInt();
        //String grade = "";
      
        //if (examScore >= 75 && examScore <= 100)
        //{
        //    grade = grade + "\nYou have achieved a Distiction!";
        //}
        //else if (examScore >= 50 && examScore <= 74)
        //{
        //    grade = grade +"\nYou have achieved a Pass!";
        //}
        // else if (examScore >= 40 && examScore <= 49)
        //{
        //    grade = grade +"\nYou have achieved a Supplementy!";
        //}
        // else if (examScore >= 0 && examScore <= 39)
        //{
        //    grade = grade +"\nYou have Unfortuanetly Failed!";
        //}
        //else
        // {
        //     System.out.println("Invalid Exam Score, please enter a score between 0 - 100!");
        //     grade = " Undefined entry";
        // }
        //System.out.println("You Have a Grade of " + examScore + grade);
        //scanner.close();
    }
    
}
