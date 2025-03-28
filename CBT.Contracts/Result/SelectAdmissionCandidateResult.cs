﻿using CBT.Contracts.Student;
using CBT.DAL.Models.Candidates;
using CBT.DAL.Models.Examinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBT.Contracts.Result
{
    public class SelectAdmissionCandidateResult
    {
        public string CandidateName { get; set; }
        public string CandidateId { get; set; }
        public string CandidateEmail { get; set; }
        public string ExaminationId { get; set; }
        public string ExaminationName { get; set; }
        public int TotalScore { get; set; }
        public string Status { get; set; }
        public SelectAdmissionCandidateResult(Candidate candidate, Examination examination, int totalScore)
        {
            CandidateId = candidate.CandidateId;
            CandidateName = $"{candidate.FirstName} {candidate.LastName}";
            CandidateEmail = candidate.Email;
            ExaminationId = examination.ExaminationId.ToString();
            ExaminationName = examination.ExamName_Subject;
            if (string.IsNullOrEmpty(examination.CandidateIds))
            {
                Status = "Not Taken";
                TotalScore = 0;
            }
            else
            {
                if (examination.CandidateIds.Split(",").Contains(candidate.Id.ToString()))
                {
                    Status = totalScore >= examination.PassMark ? "Passed" : "Failed";
                }
                else
                {
                    Status = "Not Taken";
                }
                TotalScore = totalScore;
            }
        }
    }
}
