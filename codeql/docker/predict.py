import pandas as pd
import joblib

# Load the saved model and vectorizer
model = joblib.load("/app/lr_model.pkl")
vectorizer = joblib.load("/app/vectorizer.pkl")

# Prepare labels to map model results
label_map = {0: "non sensitive", 1: "sensitive"}

# Read JSON data from file
json_data = pd.read_json('/data.json')

# Extract "results" column as DataFrame
test_data = pd.DataFrame(json_data['results'].tolist(), columns=['code', 'sourcefile'])

# Preprocess the test data
new_X = vectorizer.transform(test_data['code'])

# Probability of each class for test data
new_y_pred_prob = model.predict_proba(new_X)

# Make the predictions
new_y_pred = model.predict(new_X)

# Create a DataFrame to store the prediction results
results_df = pd.DataFrame(columns=['Entry', 'Code', 'Label', 'Confidence Sensitive', 'Confidence Non-sensitive'])

# Populate the DataFrame with prediction results
for index, (code, label, prob) in enumerate(zip(test_data['code'], new_y_pred, new_y_pred_prob)):
    confidence_sensitive = round(prob[1] * 100, 2)
    confidence_non_sensitive = round(prob[0] * 100, 2)
    label_name = label_map[label]
    
    results_df.loc[index] = [index, code, label_name, confidence_sensitive, confidence_non_sensitive]

# Save the DataFrame to a CSV file
results_df.to_csv('/app/prediction_results.csv', index=False)
