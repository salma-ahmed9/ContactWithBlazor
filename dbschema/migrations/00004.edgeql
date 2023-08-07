CREATE MIGRATION m1mc4q52i2nxoviqf3r4ht33binkuqdxgzrzklwpfjafoctd7mr6cq
    ONTO m1uwmt2gatie7rlzk4fzig4uy5kjvqiteld5y4qjy24wv5a7xdetcq
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY birth_date {
          SET TYPE std::datetime USING (<std::datetime>.birth_date);
      };
  };
};
