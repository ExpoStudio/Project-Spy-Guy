# Version: 1.0

if __name__ == '__main__':
    for tries in range(3):
        user_input1 = input("Input your exam score: ")
        user_input2 = input("Input your exam retake score: ")
        try : 
            Exam = float(user_input1)
            Retake = float(user_input2)
            final_score = max(0.5 * Exam + 0.5 * Retake, Exam)
            print('your final score is: {}'.format(final_score))
            if (final_score >= 80):
                print("Chat we not cooked!!!")
            else:
                print("You might be chopped gang")
            if ((Retake - Exam > 0)):
                print("You improved by {} points, and your grade increased by {} points!".format((Retake - Exam),(final_score - Exam)))
            else:
                print("Unfortunitely you did not improve your score")
            break
        except ValueError:
            print("Please input numbers for both fields")
    if (tries == 2):
        print("You have reached the maximum number of tries")
