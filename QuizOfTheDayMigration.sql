-- Migration script to add Quiz of the Day fields to Quiz table
-- Run this script against your database to add the new columns

-- Add IsQuizOfTheDay column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Quizzes]') AND name = 'IsQuizOfTheDay')
BEGIN
    ALTER TABLE [dbo].[Quizzes]
    ADD [IsQuizOfTheDay] bit NOT NULL DEFAULT 0
END

-- Add QuizOfTheDayDate column  
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Quizzes]') AND name = 'QuizOfTheDayDate')
BEGIN
    ALTER TABLE [dbo].[Quizzes]
    ADD [QuizOfTheDayDate] datetime2(7) NULL
END

-- Create index for efficient Quiz of the Day queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Quizzes]') AND name = 'IX_Quizzes_QuizOfTheDay')
BEGIN
    CREATE INDEX [IX_Quizzes_QuizOfTheDay] ON [dbo].[Quizzes] ([IsQuizOfTheDay], [QuizOfTheDayDate])
END

PRINT 'Quiz of the Day migration completed successfully.'