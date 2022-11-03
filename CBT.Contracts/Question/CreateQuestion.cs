﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBT.Contracts.Question
{
    public class CreateQuestion
    {
        public string QuestionText { get; set; }
        public Guid ExaminationId { get; set; }
        public int Mark { get; set; }
        public List<string> Options { get; set; }
        public string Answers { get; set; }
        public int QuestionType { get; set; }
    }
}
